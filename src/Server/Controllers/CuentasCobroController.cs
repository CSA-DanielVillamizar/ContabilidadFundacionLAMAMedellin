using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Services.CuentasCobro;
using Server.DTOs.CuentasCobro;
using Server.Data;

namespace Server.Controllers;

/// <summary>
/// Controlador para generar cuentas de cobro individuales a miembros deudores.
/// </summary>
[ApiController]
[Route("api/cuentas-cobro")]
[Authorize]
public class CuentasCobroController : ControllerBase
{
    private readonly ICuentasCobroService _cuentasCobroService;
    private readonly ILogger<CuentasCobroController> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CuentasCobroController(
        ICuentasCobroService cuentasCobroService, 
        ILogger<CuentasCobroController> logger,
        IDbContextFactory<AppDbContext> dbFactory)
    {
        _cuentasCobroService = cuentasCobroService;
        _logger = logger;
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// Genera y descarga una cuenta de cobro en PDF para un miembro específico.
    /// </summary>
    /// <param name="miembroId">ID del miembro deudor.</param>
    /// <param name="desde">Mes inicial (formato: yyyy-MM-dd, se tomará el primer día). Opcional.</param>
    /// <param name="hasta">Mes final (formato: yyyy-MM-dd, se tomará el primer día). Opcional.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>PDF de la cuenta de cobro.</returns>
    [HttpGet("{miembroId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GenerarCuentaCobro(
        Guid miembroId,
        [FromQuery] string? desde = null,
        [FromQuery] string? hasta = null,
        CancellationToken ct = default)
    {
        try
        {
            DateOnly? desdeDate = null;
            DateOnly? hastaDate = null;

            if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde, out var d))
                desdeDate = new DateOnly(d.Year, d.Month, 1);

            if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta, out var h))
                hastaDate = new DateOnly(h.Year, h.Month, 1);

            // Obtener datos del miembro para construir el nombre del archivo
            var datos = await _cuentasCobroService.ObtenerDatosCuentaCobroAsync(miembroId, desdeDate, hastaDate, ct);
            if (datos == null)
                return NotFound(new { error = "No se encontró información de deuda para el miembro especificado." });

            var pdfBytes = await _cuentasCobroService.GenerarCuentaCobroPdfAsync(miembroId, desdeDate, hastaDate, ct);

            // Extraer primer nombre y primer apellido
            var nombreCompleto = datos.NombreCompleto ?? "Miembro";
            var partesNombre = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var primerNombre = partesNombre.Length > 0 ? partesNombre[0] : "Miembro";
            var primerApellido = partesNombre.Length > 1 ? partesNombre[1] : "";
            
            var nombreArchivo = string.IsNullOrWhiteSpace(primerApellido) 
                ? $"CuentaCobro_{primerNombre}.pdf"
                : $"CuentaCobro_{primerNombre}_{primerApellido}.pdf";
            
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No se pudo generar cuenta de cobro para miembro {MiembroId}", miembroId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando cuenta de cobro para miembro {MiembroId}", miembroId);
            return StatusCode(500, new { error = "Error interno al generar la cuenta de cobro." });
        }
    }

    /// <summary>
    /// Obtiene los datos de la cuenta de cobro sin generar el PDF (útil para vista previa).
    /// </summary>
    /// <param name="miembroId">ID del miembro deudor.</param>
    /// <param name="desde">Mes inicial (formato: yyyy-MM-dd). Opcional.</param>
    /// <param name="hasta">Mes final (formato: yyyy-MM-dd). Opcional.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos estructurados de la cuenta de cobro.</returns>
    [HttpGet("{miembroId:guid}/datos")]
    public async Task<IActionResult> ObtenerDatosCuentaCobro(
        Guid miembroId,
        [FromQuery] string? desde = null,
        [FromQuery] string? hasta = null,
        CancellationToken ct = default)
    {
        try
        {
            DateOnly? desdeDate = null;
            DateOnly? hastaDate = null;

            if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde, out var d))
                desdeDate = new DateOnly(d.Year, d.Month, 1);

            if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta, out var h))
                hastaDate = new DateOnly(h.Year, h.Month, 1);

            var datos = await _cuentasCobroService.ObtenerDatosCuentaCobroAsync(miembroId, desdeDate, hastaDate, ct);

            if (datos == null)
                return NotFound(new { error = "No se encontró información de deuda para el miembro especificado." });

            return Ok(datos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo datos de cuenta de cobro para miembro {MiembroId}", miembroId);
            return StatusCode(500, new { error = "Error interno al obtener los datos." });
        }
    }

    /// <summary>
    /// Genera y descarga una cuenta de cobro en PDF a partir de una venta (solo Miembro Local).
    /// </summary>
    [HttpGet("venta/{ventaId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GenerarCuentaCobroDesdeVenta(Guid ventaId, CancellationToken ct = default)
    {
        try
        {
            await using var _db = await _dbFactory.CreateDbContextAsync(ct);
            
            var venta = await _db.VentasProductos
                .Include(v => v.Miembro)
                .FirstOrDefaultAsync(v => v.Id == ventaId, ct);

            var pdfBytes = await _cuentasCobroService.GenerarCuentaCobroDesdeVentaPdfAsync(ventaId, ct);

            var nombreArchivo = "CuentaCobro_Venta.pdf";
            if (venta?.Miembro != null)
            {
                var nombreCompleto = venta.Miembro.NombreCompleto ?? $"{venta.Miembro.Nombres} {venta.Miembro.Apellidos}";
                var partesNombre = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var primerNombre = partesNombre.Length > 0 ? partesNombre[0] : "Miembro";
                var primerApellido = partesNombre.Length > 1 ? partesNombre[1] : "";
                
                nombreArchivo = string.IsNullOrWhiteSpace(primerApellido) 
                    ? $"CuentaCobro_{primerNombre}.pdf"
                    : $"CuentaCobro_{primerNombre}_{primerApellido}.pdf";
            }

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No se pudo generar cuenta de cobro desde venta {VentaId}", ventaId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando cuenta de cobro desde venta {VentaId}", ventaId);
            return StatusCode(500, new { error = "Error interno al generar la cuenta de cobro desde venta." });
        }
    }

    /// <summary>
    /// Genera y descarga una cuenta de cobro en PDF con items personalizados.
    /// </summary>
    [HttpPost("custom")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GenerarCuentaCobroCustom([FromBody] CuentaCobroCustomRequestDto request, CancellationToken ct = default)
    {
        try
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest(new { error = "Debe enviar al menos un ítem en la solicitud." });

            await using var _db = await _dbFactory.CreateDbContextAsync(ct);
            
            var miembro = await _db.Miembros.FirstOrDefaultAsync(m => m.Id == request.MiembroId, ct);

            var pdfBytes = await _cuentasCobroService.GenerarCuentaCobroDesdeCustomPdfAsync(request, ct);

            var nombreArchivo = "CuentaCobro_Personalizada.pdf";
            if (miembro != null)
            {
                var nombreCompleto = miembro.NombreCompleto ?? $"{miembro.Nombres} {miembro.Apellidos}";
                var partesNombre = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var primerNombre = partesNombre.Length > 0 ? partesNombre[0] : "Miembro";
                var primerApellido = partesNombre.Length > 1 ? partesNombre[1] : "";
                
                nombreArchivo = string.IsNullOrWhiteSpace(primerApellido) 
                    ? $"CuentaCobro_{primerNombre}.pdf"
                    : $"CuentaCobro_{primerNombre}_{primerApellido}.pdf";
            }

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No se pudo generar cuenta de cobro personalizada para miembro {MiembroId}", request?.MiembroId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando cuenta de cobro personalizada para miembro {MiembroId}", request?.MiembroId);
            return StatusCode(500, new { error = "Error interno al generar la cuenta de cobro personalizada." });
        }
    }
}
