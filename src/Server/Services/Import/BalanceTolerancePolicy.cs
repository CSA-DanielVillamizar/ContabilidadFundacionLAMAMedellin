using System.Globalization;

namespace Server.Services.Import;

/// <summary>
/// Política de tolerancia de balance en importaciones de Excel.
/// 
/// Define la regla exacta para aceptar/rechazar movimientos de tesorería
/// durante importación en función de diferencias de balance.
/// 
/// POLÍTICA:
/// - Tolerancia: 1.00 COP (una unidad monetaria mínima)
/// - Aplicación: EXCLUSIVA (diff < 1.0, no <=)
/// - Justificación: Software contable requiere precisión.
///   Una diferencia de exactamente 1.00 es un error contable que debe reportarse.
/// 
/// EJEMPLOS:
/// - Balance esperado: 100000.00, encontrado: 100000.00 → diff=0 → ✅ ACEPTA
/// - Balance esperado: 100000.00, encontrado: 100000.99 → diff=0.99 → ✅ ACEPTA
/// - Balance esperado: 100000.00, encontrado: 100001.00 → diff=1.00 → ❌ RECHAZA
/// - Balance esperado: 100000.00, encontrado: 100000.01 → diff=0.01 → ✅ ACEPTA
/// 
/// AUDITORÍA:
/// - Esta política es la fuente única de verdad para validación de balance
/// - Cambios a la política requieren análisis de impacto contable
/// - Todos los módulos de importación deben usar esta clase
/// </summary>
public static class BalanceTolerancePolicy
{
    private static readonly CultureInfo CO = CultureInfo.GetCultureInfo("es-CO");

    /// <summary>
    /// Tolerancia de balance en moneda base (COP).
    /// Representa el máximo error permitido entre balance esperado y encontrado.
    /// </summary>
    public const decimal TOLERANCE = 1.0m;

    /// <summary>
    /// Si true: diferencia IGUAL a tolerancia es aceptada (<=)
    /// Si false: diferencia IGUAL a tolerancia es rechazada (<)
    /// 
    /// En este sistema: FALSE (exclusiva, para software contable)
    /// </summary>
    public const bool TOLERANCE_IS_INCLUSIVE = false;
    /// <summary>
    /// Calcula la diferencia absoluta entre balance esperado y encontrado.
    /// </summary>
    /// <param name="expectedBalance">Balance esperado según fuente</param>
    /// <param name="foundBalance">Balance encontrado en sistema</param>
    /// <returns>Diferencia absoluta en COP</returns>
    public static decimal CalculateDiff(decimal expectedBalance, decimal foundBalance)
    {
        return Math.Abs(expectedBalance - foundBalance);
    }


    /// <summary>
    /// Verifica si la diferencia de balance está dentro de la tolerancia definida.
    /// </summary>
    /// <param name="expectedBalance">Balance esperado según fuente</param>
    /// <param name="foundBalance">Balance encontrado en sistema</param>
    /// <returns>true si la diferencia es aceptable, false si rechaza</returns>
    public static bool IsWithinTolerance(decimal expectedBalance, decimal foundBalance)
    {
        var difference = CalculateDiff(expectedBalance, foundBalance);
        
        return TOLERANCE_IS_INCLUSIVE
            ? difference <= TOLERANCE
            : difference < TOLERANCE;
    }
    /// <summary>
    /// Formatea mensaje de mismatch con contexto completo para auditoría.
    /// </summary>
    /// <param name="context">Contexto del mismatch (ej: "Hoja MAYO 2025, fila 10")</param>
    /// <param name="expectedBalance">Balance esperado</param>
    /// <param name="foundBalance">Balance encontrado</param>
    /// <returns>Mensaje formateado con todos los detalles</returns>
    public static string FormatMismatchMessage(string context, decimal expectedBalance, decimal foundBalance)
    {
        var diff = CalculateDiff(expectedBalance, foundBalance);
        var modoStr = TOLERANCE_IS_INCLUSIVE ? "INCLUSIVA" : "EXCLUSIVA";
        var operador = TOLERANCE_IS_INCLUSIVE ? "<=" : "<";

        return string.Format(
            CO,
            "⚠️ Balance fuera de tolerancia ({0}): {1}. Esperado={2:N2} COP, Encontrado={3:N2} COP, Diff={4:N2} COP, Tolerancia={5:N2} COP. Regla: diff {6} {7:N2}",
            modoStr,
            context,
            expectedBalance,
            foundBalance,
            diff,
            TOLERANCE,
            operador,
            TOLERANCE);
    }


    /// <summary>
    /// Obtiene una descripción legible de la política para logs y reportes.
    /// </summary>
    public static string GetPolicyDescription()
    {
        var modoStr = TOLERANCE_IS_INCLUSIVE ? "INCLUSIVA" : "EXCLUSIVA";
        var operador = TOLERANCE_IS_INCLUSIVE ? "<=" : "<";
        return string.Format(
            CO,
            "Tolerancia de balance: diff {0} {1:N2} COP (modo: {2})",
            operador,
            TOLERANCE,
            modoStr);
    }
}
