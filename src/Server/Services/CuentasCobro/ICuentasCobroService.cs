using Server.DTOs.CuentasCobro;

namespace Server.Services.CuentasCobro;

/// <summary>
/// Servicio para generar cuentas de cobro (mensualidades y conceptos personalizados).
/// </summary>
public interface ICuentasCobroService
{
    /// <summary>
    /// Genera una cuenta de cobro de mensualidades en formato PDF para un miembro específico.
    /// </summary>
    Task<byte[]> GenerarCuentaCobroPdfAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene los datos estructurados de la cuenta de cobro de mensualidades sin generar el PDF.
    /// </summary>
    Task<CuentaCobroDto?> ObtenerDatosCuentaCobroAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);

    /// <summary>
    /// Genera una cuenta de cobro a partir de una venta (solo MiembroLocal) en PDF.
    /// </summary>
    Task<byte[]> GenerarCuentaCobroDesdeVentaPdfAsync(Guid ventaId, CancellationToken ct = default);

    /// <summary>
    /// Genera una cuenta de cobro personalizada desde items arbitrarios.
    /// </summary>
    Task<byte[]> GenerarCuentaCobroDesdeCustomPdfAsync(CuentaCobroCustomRequestDto request, CancellationToken ct = default);
}
