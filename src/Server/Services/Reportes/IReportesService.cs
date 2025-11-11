using Server.Models;

namespace Server.Services.Reportes;

public interface IReportesService
{
    Task<TesoreriaMesResult> GenerarReporteMensualAsync(int anio, int mes, CancellationToken ct = default);

    Task<byte[]> GenerarReporteMensualPdfAsync(int anio, int mes, CancellationToken ct = default);
    Task<byte[]> GenerarReporteMensualExcelAsync(int anio, int mes, CancellationToken ct = default);
}

public record TesoreriaMesResult(DateTime GeneradoEn, int Anio, int Mes, decimal SaldoInicial, decimal Ingresos, decimal Egresos, decimal SaldoFinal);
