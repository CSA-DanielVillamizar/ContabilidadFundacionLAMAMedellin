using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services.Egresos;

namespace Server.Controllers;

/// <summary>
/// API para gestionar egresos de tesorería (gastos) y sus soportes.
/// </summary>
[ApiController]
[Route("api/egresos")]
public class EgresosController : ControllerBase
{
    private readonly IEgresosService _egresos;

    public EgresosController(IEgresosService egresos)
    {
        _egresos = egresos;
    }

    /// <summary>
    /// Lista egresos con filtros opcionales por fecha y categoría.
    /// Accesible a Tesorero, Junta y Consulta.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<ActionResult<List<Egreso>>> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? categoria, CancellationToken ct)
    {
        var res = await _egresos.ListarAsync(desde, hasta, categoria, ct);
        return Ok(res);
    }

    /// <summary>
    /// Obtiene un egreso por Id.
    /// Accesible a Tesorero, Junta y Consulta.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<ActionResult<Egreso>> Obtener([FromRoute] Guid id, CancellationToken ct)
    {
        var eg = await _egresos.ObtenerAsync(id, ct);
        if (eg is null) return NotFound();
        return Ok(eg);
    }

    /// <summary>
    /// Crea un egreso. Requiere rol Tesorero o Junta.
    /// Soporta carga de archivo (multipart/form-data) en el campo "soporte".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "TesoreroJunta")]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<ActionResult<Egreso>> Crear([FromForm] EgresoCreateRequest request, CancellationToken ct)
    {
        var egreso = new Egreso
        {
            Fecha = request.Fecha,
            Categoria = request.Categoria ?? string.Empty,
            Proveedor = request.Proveedor ?? string.Empty,
            Descripcion = request.Descripcion ?? string.Empty,
            ValorCop = request.ValorCop
        };

        var usuario = User?.Identity?.Name ?? "system";
        var creado = await _egresos.CrearAsync(egreso, request.Soporte, usuario, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// Actualiza un egreso. Requiere rol Tesorero o Junta.
    /// Soporta reemplazo de soporte.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "TesoreroJunta")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<Egreso>> Actualizar([FromRoute] Guid id, [FromForm] EgresoUpdateRequest request, CancellationToken ct)
    {
        var egreso = new Egreso
        {
            Fecha = request.Fecha,
            Categoria = request.Categoria ?? string.Empty,
            Proveedor = request.Proveedor ?? string.Empty,
            Descripcion = request.Descripcion ?? string.Empty,
            ValorCop = request.ValorCop
        };

        var usuario = User?.Identity?.Name ?? "system";
        var actualizado = await _egresos.ActualizarAsync(id, egreso, request.Soporte, usuario, ct);
        if (actualizado is null) return NotFound();
        return Ok(actualizado);
    }

    /// <summary>
    /// Elimina un egreso y su soporte. Requiere rol Tesorero o Junta.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "TesoreroJunta")]
    public async Task<IActionResult> Eliminar([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _egresos.EliminarAsync(id, ct);
        if (!ok) return NotFound();
        return NoContent();
    }
}

/// <summary>
/// Modelo para creación de egresos vía multipart/form-data.
/// </summary>
public class EgresoCreateRequest
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Categoria { get; set; }
    public string? Proveedor { get; set; }
    public string? Descripcion { get; set; }
    public decimal ValorCop { get; set; }
    public IFormFile? Soporte { get; set; }
}

/// <summary>
/// Modelo para actualización de egresos vía multipart/form-data.
/// </summary>
public class EgresoUpdateRequest
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Categoria { get; set; }
    public string? Proveedor { get; set; }
    public string? Descripcion { get; set; }
    public decimal ValorCop { get; set; }
    public IFormFile? Soporte { get; set; }
}
