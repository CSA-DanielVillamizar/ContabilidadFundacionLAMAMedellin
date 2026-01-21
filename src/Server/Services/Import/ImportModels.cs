namespace Server.Services.Import;

/// <summary>
/// Resumen de una operación de importación desde Excel
/// </summary>
public class ImportSummary
{
    public bool Success { get; set; }
    public int TotalRowsProcessed { get; set; }
    public int MovimientosImported { get; set; }
    public int MovimientosSkipped { get; set; }
    public int BalanceMismatches { get; set; }
    public decimal? SaldoFinalCalculado { get; set; }
    public decimal? SaldoFinalEsperado { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, int> MovimientosPorHoja { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Configuración de importación
/// </summary>
public class ImportOptions
{
    public string TreasuryExcelPath { get; set; } = "INFORME TESORERIA.xlsx";
    public bool Enabled { get; set; } = true;
}
