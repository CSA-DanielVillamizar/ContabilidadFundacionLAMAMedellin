using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Data;
using Server.Models;

namespace Server.Services.Import;

/// <summary>
/// Servicio para importar histórico de tesorería desde Excel (INFORME TESORERIA.xlsx).
/// Implementa idempotencia vía hash, validación de saldos, y trazabilidad completa.
/// </summary>
public interface IExcelTreasuryImportService
{
    Task<ImportSummary> ImportAsync(string? filePath = null, bool dryRun = false);
}

public class ExcelTreasuryImportService : IExcelTreasuryImportService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<ExcelTreasuryImportService> _logger;
    private readonly ImportOptions _options;

    public ExcelTreasuryImportService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<ExcelTreasuryImportService> logger,
        IOptions<ImportOptions> options)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ImportSummary> ImportAsync(string? filePath = null, bool dryRun = false)
    {
        var summary = new ImportSummary();
        filePath ??= _options.TreasuryExcelPath;

        if (!File.Exists(filePath))
        {
            summary.Errors.Add($"Archivo no encontrado: {filePath}");
            return summary;
        }

        using var workbook = new XLWorkbook(filePath);
        using var db = await _dbFactory.CreateDbContextAsync();

        // Obtener o crear cuenta Bancolombia
        var cuenta = await db.CuentasFinancieras
            .FirstOrDefaultAsync(c => c.Codigo == "BANCO-BCOL-001");
        if (cuenta == null)
        {
            summary.Errors.Add("Cuenta BANCO-BCOL-001 no existe. Ejecutar migración primero.");
            return summary;
        }

        // Obtener catálogos
        var fuentes = await db.FuentesIngreso.ToDictionaryAsync(f => f.Codigo);
        var categorias = await db.CategoriasEgreso.ToDictionaryAsync(c => c.Codigo);

        var saldoAcumulado = cuenta.SaldoInicial;

        // Detectar y procesar hojas en orden cronológico
        var hojas = DetectTreasurySheets(workbook).OrderBy(h => h.fecha).ToList();
        if (hojas.Count == 0)
        {
            summary.Warnings.Add("No se encontraron hojas con formato reconocible (CORTE)");
            return summary;
        }

        foreach (var (sheet, fecha) in hojas)
        {
            var movimientos = ParseMovimientosFromSheet(sheet, cuenta.Id, fuentes, categorias, summary);
            summary.MovimientosPorHoja[sheet.Name] = movimientos.Count;

            // Validar saldo por fila
            var saldoInicio = saldoAcumulado;
            foreach (var mov in movimientos)
            {
                // Calcular saldo esperado
                saldoAcumulado += mov.Tipo == TipoMovimientoTesoreria.Ingreso ? mov.Valor : -mov.Valor;
                
                // Verificar mismatch (tolerancia ±1 COP)
                if (mov.ImportBalanceExpected.HasValue)
                {
                    var diff = Math.Abs(saldoAcumulado - mov.ImportBalanceExpected.Value);
                    if (diff > 1m)
                    {
                        mov.ImportHasBalanceMismatch = true;
                        mov.ImportBalanceFound = saldoAcumulado;
                        summary.BalanceMismatches++;
                        summary.Warnings.Add($"Hoja {sheet.Name}, fila {mov.ImportRowNumber}: " +
                            $"Saldo esperado {mov.ImportBalanceExpected:N0}, encontrado {saldoAcumulado:N0}");
                    }
                }
            }

            // Importar movimientos con idempotencia
            if (!dryRun)
            {
                foreach (var mov in movimientos)
                {
                    var exists = await db.MovimientosTesoreria.AnyAsync(m => m.ImportHash == mov.ImportHash);
                    if (!exists)
                    {
                        db.MovimientosTesoreria.Add(mov);
                        summary.MovimientosImported++;
                    }
                    else
                    {
                        summary.MovimientosSkipped++;
                    }
                }
                await db.SaveChangesAsync();
            }
            else
            {
                summary.MovimientosImported += movimientos.Count;
            }

            summary.TotalRowsProcessed += movimientos.Count;
        }

        summary.SaldoFinalCalculado = saldoAcumulado;
        summary.Success = summary.Errors.Count == 0;
        summary.Message = dryRun 
            ? $"Dry Run: {summary.MovimientosImported} movimientos serían importados"
            : $"Importación completa: {summary.MovimientosImported} movimientos creados, {summary.MovimientosSkipped} ya existían";

        return summary;
    }

    /// <summary>
    /// Detecta hojas con formato de tesorería (nombres tipo "CORTE MAYO - 24", "CORTE A MAYO 2024", etc)
    /// </summary>
    private List<(IXLWorksheet sheet, DateTime fecha)> DetectTreasurySheets(XLWorkbook workbook)
    {
        var result = new List<(IXLWorksheet, DateTime)>();
        // Regex mejorado: captura el último número como año (ej: "NOVIEMBRE 30-25" => 25)
        var regex = new Regex(@"CORTE\s+(A\s+)?(?<mes>\w+)[\s\-\.]+(?:\d+[\s\-])?(?<ano>\d{2,4})\s*$", RegexOptions.IgnoreCase);

        foreach (var sheet in workbook.Worksheets)
        {
            var match = regex.Match(sheet.Name);
            if (match.Success)
            {
                var mesStr = match.Groups["mes"].Value;
                var anoStr = match.Groups["ano"].Value;
                if (TryParseMesAno(mesStr, anoStr, out var fecha))
                {
                    result.Add((sheet, fecha));
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Parsea mes y año desde texto (ej: "MAYO - 24" => mayo 2024)
    /// </summary>
    private bool TryParseMesAno(string mesStr, string anoStr, out DateTime fecha)
    {
        fecha = DateTime.MinValue;
        var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["enero"] = 1, ["febrero"] = 2, ["marzo"] = 3, ["abril"] = 4,
            ["mayo"] = 5, ["junio"] = 6, ["julio"] = 7, ["agosto"] = 8,
            ["septiembre"] = 9, ["octubre"] = 10, ["noviembre"] = 11, ["diciembre"] = 12
        };

        if (!meses.TryGetValue(mesStr.Trim(), out var mes))
            return false;

        if (!int.TryParse(anoStr, out var ano))
            return false;

        // Convertir año 2 dígitos a 4
        if (ano < 100)
            ano += 2000;

        fecha = new DateTime(ano, mes, 1);
        return true;
    }

    /// <summary>
    /// Lee movimientos de una hoja, detectando encabezado y excluyendo filas resumen
    /// </summary>
    private List<MovimientoTesoreria> ParseMovimientosFromSheet(
        IXLWorksheet sheet, 
        Guid cuentaId, 
        Dictionary<string, FuenteIngreso> fuentes,
        Dictionary<string, CategoriaEgreso> categorias,
        ImportSummary summary)
    {
        var movimientos = new List<MovimientoTesoreria>();

        // Detectar fila de encabezado (buscar "FECHA", "CONCEPTO", "INGRESOS", "EGRESOS", "SALDO")
        IXLRow? headerRow = null;
        int colFecha = -1, colConcepto = -1, colIngresos = -1, colEgresos = -1, colSaldo = -1;

        for (int r = 1; r <= Math.Min(20, sheet.LastRowUsed()?.RowNumber() ?? 20); r++)
        {
            var row = sheet.Row(r);
            for (int c = 1; c <= Math.Min(10, sheet.LastColumnUsed()?.ColumnNumber() ?? 10); c++)
            {
                var cell = row.Cell(c);
                var val = cell.GetString().Trim().ToUpper();
                if (val == "FECHA") colFecha = c;
                if (val == "CONCEPTO") colConcepto = c;
                if (val == "INGRESOS") colIngresos = c;
                if (val == "EGRESOS") colEgresos = c;
                if (val == "SALDO") colSaldo = c;
            }
            if (colFecha > 0 && colConcepto > 0 && colIngresos > 0 && colEgresos > 0)
            {
                headerRow = row;
                break;
            }
        }

        if (headerRow == null)
        {
            summary.Warnings.Add($"Hoja {sheet.Name}: No se encontró encabezado con columnas esperadas");
            return movimientos;
        }

        // Leer filas hasta encontrar filas resumen o vacías
        var startRow = headerRow.RowNumber() + 1;
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? startRow;

        for (int r = startRow; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            var fechaCell = row.Cell(colFecha);
            var conceptoCell = row.Cell(colConcepto);
            var ingresosCell = row.Cell(colIngresos);
            var egresosCell = row.Cell(colEgresos);
            var saldoCell = colSaldo > 0 ? row.Cell(colSaldo) : null;

            // Detectar fila resumen (SALDO EFECTIVO, TOTAL INGRESOS, etc)
            var concepto = conceptoCell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(concepto) || IsResumenRow(concepto))
                continue;

            // Parsear fecha
            if (!TryParseDate(fechaCell, out var fecha))
                continue;

            // Parsear valores
            var ingresos = ParseDecimal(ingresosCell);
            var egresos = ParseDecimal(egresosCell);
            var saldo = saldoCell != null ? ParseDecimal(saldoCell) : (decimal?)null;

            // Validar: debe tener ingreso XOR egreso
            if ((ingresos <= 0 && egresos <= 0) || (ingresos > 0 && egresos > 0))
                continue;

            var tipo = ingresos > 0 ? TipoMovimientoTesoreria.Ingreso : TipoMovimientoTesoreria.Egreso;
            var valor = ingresos > 0 ? ingresos : egresos;

            // Clasificar
            var (fuenteId, categoriaId) = ClasificarMovimiento(concepto, tipo, fuentes, categorias, summary);

            // Crear movimiento
            var mov = new MovimientoTesoreria
            {
                Id = Guid.NewGuid(),
                NumeroMovimiento = $"IMP-{fecha:yyyy-MM}-{r:D4}",
                Fecha = fecha,
                Tipo = tipo,
                CuentaFinancieraId = cuentaId,
                FuenteIngresoId = fuenteId,
                CategoriaEgresoId = categoriaId,
                Valor = valor,
                Descripcion = concepto,
                Medio = MedioPagoTesoreria.Transferencia,
                Estado = EstadoMovimientoTesoreria.Aprobado,
                FechaAprobacion = fecha,
                UsuarioAprobacion = "import",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "import",
                ImportHash = ComputeHash(fecha, concepto, tipo, valor, saldo, sheet.Name),
                ImportSource = "INFORME TESORERIA.xlsx",
                ImportSheet = sheet.Name,
                ImportRowNumber = r,
                ImportedAtUtc = DateTime.UtcNow,
                ImportBalanceExpected = saldo
            };

            movimientos.Add(mov);
        }

        return movimientos;
    }

    /// <summary>
    /// Detecta si una fila es resumen (no debe importarse como movimiento)
    /// </summary>
    private bool IsResumenRow(string concepto)
    {
        var keywords = new[] { 
            "SALDO EFECTIVO", "TOTAL INGRESOS", "INGRESOS DOLARES", "EGRESOS", 
            "SALDO EN TESORERIA", "MES ANTERIOR", "TOTAL EGRESOS", "SALDO FINAL"
        };
        var upper = concepto.ToUpper();
        return keywords.Any(k => upper.Contains(k));
    }

    /// <summary>
    /// Intenta parsear fecha desde celda (puede ser DateTime o texto)
    /// </summary>
    private bool TryParseDate(IXLCell cell, out DateTime fecha)
    {
        fecha = DateTime.MinValue;
        try
        {
            if (cell.TryGetValue(out DateTime dt))
            {
                fecha = dt;
                return true;
            }
            var str = cell.GetString().Trim();
            if (DateTime.TryParse(str, out dt))
            {
                fecha = dt;
                return true;
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Parsea decimal desde celda (maneja formato colombiano: 1.000.000,50)
    /// </summary>
    private decimal ParseDecimal(IXLCell cell)
    {
        try
        {
            if (cell.TryGetValue(out double dbl))
                return (decimal)dbl;
            
            // Formato colombiano: punto = separador de miles, coma = decimal
            var str = cell.GetString().Trim()
                .Replace("$", "")
                .Replace(" ", "")
                .Replace(".", "");  // Remover separador de miles
            
            // Reemplazar coma decimal por punto para parsear
            str = str.Replace(",", ".");
            
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
        }
        catch { }
        return 0m;
    }

    /// <summary>
    /// Clasifica movimiento por palabras clave del concepto
    /// </summary>
    private (Guid? fuenteId, Guid? categoriaId) ClasificarMovimiento(
        string concepto, 
        TipoMovimientoTesoreria tipo, 
        Dictionary<string, FuenteIngreso> fuentes,
        Dictionary<string, CategoriaEgreso> categorias,
        ImportSummary summary)
    {
        var upper = concepto.ToUpper();

        if (tipo == TipoMovimientoTesoreria.Ingreso)
        {
            // Mapeo de palabras clave a fuentes
            if (upper.Contains("APORTE") || upper.Contains("MENSUALIDAD"))
                return (fuentes["APORTE-MEN"].Id, null);
            if (upper.Contains("DONACION") || upper.Contains("DONACIÓN"))
                return (fuentes["DONACION"].Id, null);
            if (upper.Contains("MERCHANDISING") || upper.Contains("VENTA MERCH"))
                return (fuentes["VENTA-MERCH"].Id, null);
            if (upper.Contains("CLUB CAFE") || upper.Contains("CAFÉ"))
                return (fuentes["VENTA-CLUB-CAFE"].Id, null);
            if (upper.Contains("CLUB CERV") || upper.Contains("CERVEZA"))
                return (fuentes["VENTA-CLUB-CERV"].Id, null);
            if (upper.Contains("CLUB COMI") || upper.Contains("COMIDA"))
                return (fuentes["VENTA-CLUB-COMI"].Id, null);
            if (upper.Contains("EVENTO"))
                return (fuentes["EVENTO"].Id, null);
            if (upper.Contains("RENOVACION") || upper.Contains("RENOVACIÓN"))
                return (fuentes["RENOVACION-MEM"].Id, null);

            // Fallback a OTROS
            return (fuentes["OTROS"].Id, null);
        }
        else
        {
            // Mapeo de palabras clave a categorías
            if (upper.Contains("AYUDA SOCIAL") || upper.Contains("AYUDA"))
                return (null, categorias["AYUDA-SOCIAL"].Id);
            if (upper.Contains("EVENTO") || upper.Contains("LOGISTICA"))
                return (null, categorias["EVENTO-LOG"].Id);
            if (upper.Contains("PAPELERIA") || upper.Contains("ÚTILES"))
                return (null, categorias["ADMIN-PAPEL"].Id);
            if (upper.Contains("TRANSPORTE") || upper.Contains("DESPLAZAMIENTO"))
                return (null, categorias["ADMIN-TRANSP"].Id);
            if (upper.Contains("SERVICIO") || upper.Contains("PÚBLICO"))
                return (null, categorias["ADMIN-SERVICIOS"].Id);
            if (upper.Contains("MANTENIMIENTO") || upper.Contains("REPARACION"))
                return (null, categorias["MANTENIMIENTO"].Id);
            if (upper.Contains("CAFE") || upper.Contains("CAFÉ"))
                return (null, categorias["COMPRA-CLUB-CAFE"].Id);
            if (upper.Contains("CERVEZA"))
                return (null, categorias["COMPRA-CLUB-CERV"].Id);
            if (upper.Contains("COMIDA"))
                return (null, categorias["COMPRA-CLUB-COMI"].Id);
            if (upper.Contains("MERCH"))
                return (null, categorias["COMPRA-MERCH"].Id);

            // Fallback a OTROS-GASTOS
            return (null, categorias["OTROS-GASTOS"].Id);
        }
    }

    /// <summary>
    /// Calcula hash SHA256 para idempotencia
    /// </summary>
    private string ComputeHash(DateTime fecha, string concepto, TipoMovimientoTesoreria tipo, decimal valor, decimal? saldo, string sheet)
    {
        var data = $"{fecha:yyyy-MM-dd}|{concepto.Trim()}|{tipo}|{valor}|{saldo}|{sheet}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes);
    }
}
