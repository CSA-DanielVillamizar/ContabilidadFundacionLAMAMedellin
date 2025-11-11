namespace Server.Services.Deudores;

/// <summary>
/// Servicio para generar exportaciones (Excel, PDF) de deudores.
/// </summary>
public interface IDeudoresExportService
{
    /// <summary>
    /// Genera un archivo Excel con el listado de deudores y filtros aplicados.
    /// </summary>
    /// <param name="desde">Mes de inicio (yyyy-MM-dd). Null = sin límite inferior.</param>
    /// <param name="hasta">Mes de fin (yyyy-MM-dd). Null = sin límite superior.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Bytes del archivo Excel generado.</returns>
    Task<byte[]> GenerarExcelAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);

    /// <summary>
    /// Genera un archivo PDF con el listado de deudores y filtros aplicados.
    /// </summary>
    /// <param name="desde">Mes de inicio (yyyy-MM-dd). Null = sin límite inferior.</param>
    /// <param name="hasta">Mes de fin (yyyy-MM-dd). Null = sin límite superior.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Bytes del archivo PDF generado.</returns>
    Task<byte[]> GenerarPdfAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}
