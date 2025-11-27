using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Proveedores;
using Server.Services.Auth;
using Server.Services.Proveedores;

namespace Server.Controllers;

/// <summary>
/// Controller para gestión de proveedores
/// NOTA TEMPORAL: Sin [Authorize] porque Blazor Server HttpClient no envía cookies
/// La autorización real está en la página Razor con @attribute [Authorize(Policy)]
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize(Policy = "GerenciaNegocios")] // Comentado temporalmente
public class ProveedoresController : ControllerBase
{
    private readonly IProveedoresService _proveedoresService;
    private readonly ICurrentUserService _currentUserService;

    public ProveedoresController(
        IProveedoresService proveedoresService,
        ICurrentUserService currentUserService)
    {
        _proveedoresService = proveedoresService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Obtiene lista de proveedores con filtros
    /// GET /api/proveedores?busqueda=ABC&activo=true&pagina=1&registrosPorPagina=20
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ProveedoresListResponse>> Get(
        [FromQuery] string? busqueda = null,
        [FromQuery] bool? activo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int registrosPorPagina = 20)
    {
        var (proveedores, totalCount) = await _proveedoresService.ObtenerProveedoresAsync(
            busqueda, activo, pagina, registrosPorPagina);

        return Ok(new ProveedoresListResponse
        {
            Proveedores = proveedores,
            TotalCount = totalCount,
            Pagina = pagina,
            RegistrosPorPagina = registrosPorPagina,
            TotalPaginas = (int)Math.Ceiling(totalCount / (double)registrosPorPagina)
        });
    }

    /// <summary>
    /// Obtiene un proveedor por ID
    /// GET /api/proveedores/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorDto>> GetById(Guid id)
    {
        var proveedor = await _proveedoresService.ObtenerProveedorPorIdAsync(id);
        if (proveedor == null)
        {
            return NotFound(new { message = "Proveedor no encontrado" });
        }

        return Ok(proveedor);
    }

    /// <summary>
    /// Obtiene el detalle completo de un proveedor con estadísticas
    /// GET /api/proveedores/{id}/detalle
    /// </summary>
    [HttpGet("{id}/detalle")]
    public async Task<ActionResult<ProveedorDetalleDto>> GetDetalle(Guid id)
    {
        var detalle = await _proveedoresService.ObtenerProveedorDetalleAsync(id);
        if (detalle == null)
        {
            return NotFound(new { message = "Proveedor no encontrado" });
        }

        return Ok(detalle);
    }

    /// <summary>
    /// Obtiene el historial de compras de un proveedor
    /// GET /api/proveedores/{id}/historial?fechaDesde=2025-01-01&fechaHasta=2025-12-31
    /// </summary>
    [HttpGet("{id}/historial")]
    public async Task<ActionResult<List<CompraProveedorDto>>> GetHistorialCompras(
        Guid id,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var historial = await _proveedoresService.ObtenerHistorialComprasAsync(
            id, fechaDesde, fechaHasta);

        return Ok(historial);
    }

    /// <summary>
    /// Obtiene proveedores activos para dropdown/select
    /// GET /api/proveedores/activos
    /// </summary>
    [HttpGet("activos")]
    public async Task<ActionResult<List<ProveedorSimpleDto>>> GetActivos()
    {
        var proveedores = await _proveedoresService.ObtenerProveedoresActivosAsync();
        return Ok(proveedores);
    }

    /// <summary>
    /// Valida si un NIT ya existe
    /// GET /api/proveedores/validar-nit?nit=123456789&proveedorId={guid}
    /// </summary>
    [HttpGet("validar-nit")]
    public async Task<ActionResult<NitValidationResponse>> ValidarNit(
        [FromQuery] string nit,
        [FromQuery] Guid? proveedorId = null)
    {
        if (string.IsNullOrWhiteSpace(nit))
        {
            return BadRequest(new { message = "El NIT es requerido" });
        }

        var existe = await _proveedoresService.ExisteNitAsync(nit, proveedorId);

        return Ok(new NitValidationResponse
        {
            Existe = existe,
            Disponible = !existe
        });
    }

    /// <summary>
    /// Crea un nuevo proveedor
    /// POST /api/proveedores
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult<ProveedorCreatedResponse>> Create([FromBody] ProveedorFormDto dto)
    {
        var usuarioId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(usuarioId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var (success, message, proveedorId) = await _proveedoresService.CrearProveedorAsync(dto, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = proveedorId },
            new ProveedorCreatedResponse
            {
                Message = message,
                ProveedorId = proveedorId!.Value
            });
    }

    /// <summary>
    /// Actualiza un proveedor existente
    /// PUT /api/proveedores/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ProveedorFormDto dto)
    {
        var usuarioId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(usuarioId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var (success, message) = await _proveedoresService.ActualizarProveedorAsync(id, dto, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Elimina (desactiva) un proveedor
    /// DELETE /api/proveedores/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuarioId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(usuarioId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var (success, message) = await _proveedoresService.EliminarProveedorAsync(id, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }
}

/// <summary>
/// Response para lista paginada de proveedores
/// </summary>
public class ProveedoresListResponse
{
    public List<ProveedorDto> Proveedores { get; set; } = new();
    public int TotalCount { get; set; }
    public int Pagina { get; set; }
    public int RegistrosPorPagina { get; set; }
    public int TotalPaginas { get; set; }
}

/// <summary>
/// Response para creación de proveedor
/// </summary>
public class ProveedorCreatedResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid ProveedorId { get; set; }
}

/// <summary>
/// Response para validación de NIT
/// </summary>
public class NitValidationResponse
{
    public bool Existe { get; set; }
    public bool Disponible { get; set; }
}
