using System.Threading;
using System.Threading.Tasks;

namespace Server.Services.Reportes;

/// <summary>
/// Servicio de verificación automática de tesorería.
/// Provee métodos para validar saldos y reparar condiciones mínimas (saldo inicial).
/// </summary>
public interface IVerificacionTesoreriaService
{
    /// <summary>
    /// Verifica los saldos del período (año/mes) calculando saldo inicial, ingresos, egresos y saldo final.
    /// </summary>
    Task<VerificacionResultado> VerificarAsync(int anio, int mes, CancellationToken ct = default);

    /// <summary>
    /// Asegura que el Recibo de saldo inicial (AJUSTE-2025-0001) tenga su item de SALDO_INICIAL.
    /// Devuelve true si realizó una reparación.
    /// </summary>
    Task<bool> RepararSaldoInicialAsync(CancellationToken ct = default);
}

/// <summary>
/// Resultado de verificación de tesorería mensual.
/// </summary>
public record VerificacionResultado(
    int Anio,
    int Mes,
    decimal SaldoInicial,
    decimal Ingresos,
    decimal Egresos,
    decimal SaldoFinal,
    bool SaldoInicialItemExiste,
    bool MonedaOrigenConsistente
);
