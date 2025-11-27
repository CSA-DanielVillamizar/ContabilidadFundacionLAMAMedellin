using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Cotizaciones;
using Server.Services.Auth;
using Server.Services.Cotizaciones;

namespace Server.Controllers;

/// <summary>
/// Controlador para gestión de cotizaciones.
/// NOTA TEMPORAL: Sin [Authorize] porque Blazor Server HttpClient no envía cookies
/// </summary>
// [Authorize(Roles = "Admin,Gerente,gerentenegocios,Tesorero")] // Comentado temporalmente
[ApiController]
[Route("api/[controller]")]
public class CotizacionesController : ControllerBase
{
    private readonly ICotizacionesService _service;
    private readonly ICurrentUserService _currentUser;

    public CotizacionesController(ICotizacionesService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Obtiene listado de cotizaciones con filtros y paginación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCotizaciones(
        [FromQuery] string? busqueda = null,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] Guid? miembroId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int registrosPorPagina = 20)
    {
        var (cotizaciones, totalCount) = await _service.ObtenerCotizacionesAsync(
            busqueda, estado, clienteId, miembroId, desde, hasta, pagina, registrosPorPagina);
        return Ok(new { Cotizaciones = cotizaciones, TotalCount = totalCount });
    }

    /// <summary>
    /// Obtiene detalle completo de una cotización.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        var detalle = await _service.ObtenerDetalleAsync(id);
        if (detalle == null) return NotFound("Cotización no encontrada.");
        return Ok(detalle);
    }

    /// <summary>
    /// Crea una nueva cotización.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CotizacionFormDto dto)
    {
        var userId = _currentUser.GetUserId() ?? "system";
        var (success, message, cotizacionId) = await _service.CrearCotizacionAsync(dto, userId);
        if (!success) return BadRequest(message);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id = cotizacionId }, new { Id = cotizacionId, Message = message });
    }

    /// <summary>
    /// Actualiza una cotización existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] CotizacionFormDto dto)
    {
        var userId = _currentUser.GetUserId() ?? "system";
        var (success, message) = await _service.ActualizarCotizacionAsync(id, dto, userId);
        if (!success) return BadRequest(message);
        return Ok(message);
    }

    /// <summary>
    /// Cambia el estado de una cotización.
    /// </summary>
    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoCotizacionDto dto)
    {
        var userId = _currentUser.GetUserId() ?? "system";
        var (success, message) = await _service.CambiarEstadoAsync(id, dto.Estado, userId);
        if (!success) return BadRequest(message);
        return Ok(message);
    }

    /// <summary>
    /// Elimina una cotización (si permitido por estado).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var userId = _currentUser.GetUserId() ?? "system";
        var (success, message) = await _service.EliminarCotizacionAsync(id, userId);
        if (!success) return BadRequest(message);
        return Ok(message);
    }

    /// <summary>
    /// Genera un nuevo número de cotización.
    /// </summary>
    [HttpGet("generar-numero")]
    public async Task<IActionResult> GenerarNumero([FromQuery] DateTime? fecha = null)
    {
        var numero = await _service.GenerarNumeroAsync(fecha ?? DateTime.UtcNow);
        return Ok(new { Numero = numero });
    }
}

/// <summary>
/// DTO para cambiar estado de cotización.
/// </summary>
public class CambiarEstadoCotizacionDto
{
    public string Estado { get; set; } = string.Empty;
}
