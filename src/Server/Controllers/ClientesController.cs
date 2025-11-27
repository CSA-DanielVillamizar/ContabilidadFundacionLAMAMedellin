using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Clientes;
using Server.Services.Auth;
using Server.Services.Clientes;

namespace Server.Controllers;

/// <summary>
/// Controller para gestión de clientes
/// NOTA TEMPORAL: Sin [Authorize] porque Blazor Server HttpClient no envía cookies
/// La autorización real está en la página Razor con @attribute [Authorize(Policy)]
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize(Policy = "GerenciaNegocios")] // Comentado temporalmente - ver nota arriba
public class ClientesController : ControllerBase
{
    private readonly IClientesService _clientesService;
    private readonly ICurrentUserService _currentUserService;

    public ClientesController(
        IClientesService clientesService,
        ICurrentUserService currentUserService)
    {
        _clientesService = clientesService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Obtiene lista de clientes con filtros
    /// GET /api/clientes?busqueda=ABC&activo=true&tipo=Natural&pagina=1&registrosPorPagina=20
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ClientesListResponse>> Get(
        [FromQuery] string? busqueda = null,
        [FromQuery] bool? activo = null,
        [FromQuery] string? tipo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int registrosPorPagina = 20)
    {
        var (clientes, totalCount) = await _clientesService.ObtenerClientesAsync(
            busqueda, activo, tipo, pagina, registrosPorPagina);

        return Ok(new ClientesListResponse
        {
            Clientes = clientes,
            TotalCount = totalCount,
            Pagina = pagina,
            RegistrosPorPagina = registrosPorPagina,
            TotalPaginas = (int)Math.Ceiling(totalCount / (double)registrosPorPagina)
        });
    }

    /// <summary>
    /// Obtiene un cliente por ID
    /// GET /api/clientes/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(Guid id)
    {
        var cliente = await _clientesService.ObtenerClientePorIdAsync(id);
        if (cliente == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        return Ok(cliente);
    }

    /// <summary>
    /// Obtiene el detalle completo de un cliente con estadísticas
    /// GET /api/clientes/{id}/detalle
    /// </summary>
    [HttpGet("{id}/detalle")]
    public async Task<ActionResult<ClienteDetalleDto>> GetDetalle(Guid id)
    {
        var detalle = await _clientesService.ObtenerClienteDetalleAsync(id);
        if (detalle == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        return Ok(detalle);
    }

    /// <summary>
    /// Obtiene el historial de ventas de un cliente
    /// GET /api/clientes/{id}/historial?fechaDesde=2025-01-01&fechaHasta=2025-12-31
    /// </summary>
    [HttpGet("{id}/historial")]
    public async Task<ActionResult<List<VentaClienteDto>>> GetHistorialVentas(
        Guid id,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var historial = await _clientesService.ObtenerHistorialVentasAsync(
            id, fechaDesde, fechaHasta);

        return Ok(historial);
    }

    /// <summary>
    /// Obtiene clientes activos para dropdown/select
    /// GET /api/clientes/activos
    /// </summary>
    [HttpGet("activos")]
    public async Task<ActionResult<List<ClienteSimpleDto>>> GetActivos()
    {
        var clientes = await _clientesService.ObtenerClientesActivosAsync();
        return Ok(clientes);
    }

    /// <summary>
    /// Valida si una identificación ya existe
    /// GET /api/clientes/validar-identificacion?identificacion=123456789&clienteId={guid}
    /// </summary>
    [HttpGet("validar-identificacion")]
    public async Task<ActionResult<IdentificacionValidationResponse>> ValidarIdentificacion(
        [FromQuery] string identificacion,
        [FromQuery] Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
        {
            return BadRequest(new { message = "La identificación es requerida" });
        }

        var existe = await _clientesService.ExisteIdentificacionAsync(identificacion, clienteId);

        return Ok(new IdentificacionValidationResponse
        {
            Existe = existe,
            Disponible = !existe
        });
    }

    /// <summary>
    /// Crea un nuevo cliente
    /// POST /api/clientes
    /// </summary>
    [HttpPost]
    // Aceptar tanto el rol histórico "Gerente" como el rol real "gerentenegocios" utilizando policy.
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult<ClienteCreatedResponse>> Create([FromBody] ClienteFormDto dto)
    {
           var usuarioId = _currentUserService.GetUserId() ?? string.Empty;
        var (success, message, clienteId) = await _clientesService.CrearClienteAsync(dto, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetById), new { id = clienteId }, new ClienteCreatedResponse
        {
            Success = true,
            Message = message,
            ClienteId = clienteId!.Value
        });
    }

    /// <summary>
    /// Actualiza un cliente existente
    /// PUT /api/clientes/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ClienteFormDto dto)
    {
           var usuarioId = _currentUserService.GetUserId() ?? string.Empty;
        var (success, message) = await _clientesService.ActualizarClienteAsync(id, dto, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { success = true, message });
    }

    /// <summary>
    /// Elimina (desactiva) un cliente
    /// DELETE /api/clientes/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "GerenciaNegocios")]
    public async Task<ActionResult> Delete(Guid id)
    {
           var usuarioId = _currentUserService.GetUserId() ?? string.Empty;
        var (success, message) = await _clientesService.EliminarClienteAsync(id, usuarioId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { success = true, message });
    }
}

/// <summary>
/// Response para listado de clientes
/// </summary>
public class ClientesListResponse
{
    public List<ClienteDto> Clientes { get; set; } = new();
    public int TotalCount { get; set; }
    public int Pagina { get; set; }
    public int RegistrosPorPagina { get; set; }
    public int TotalPaginas { get; set; }
}

/// <summary>
/// Response para creación de cliente
/// </summary>
public class ClienteCreatedResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
}

/// <summary>
/// Response para validación de identificación
/// </summary>
public class IdentificacionValidationResponse
{
    public bool Existe { get; set; }
    public bool Disponible { get; set; }
}
