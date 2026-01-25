using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Data;
using Server.Models;
using Server.Services.MovimientosTesoreria;

namespace Server.Services.Import;

/// <summary>
/// Servicio para importar histórico de tesorería desde Excel (INFORME TESORERIA.xlsx).
/// Implementa idempotencia vía hash, validación de saldos, y trazabilidad completa.
/// </summary>
public interface IExcelTreasuryImportService
{
    Task<ImportSummary> ImportAsync(string? filePath = null, bool dryRun = false);    Task<ImportSummary> ImportAsync(Stream excelStream, string sourceName, bool dryRun = false);}

public class ExcelTreasuryImportService : IExcelTreasuryImportService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<ExcelTreasuryImportService> _logger;
    private readonly ImportOptions _options;
    private readonly CierreContable.CierreContableService _cierreService;
    private readonly MovimientosTesoreriaService _movimientosService;

    public ExcelTreasuryImportService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<ExcelTreasuryImportService> logger,
        IOptions<ImportOptions> options,
        CierreContable.CierreContableService cierreService,
        MovimientosTesoreriaService movimientosService)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _options = options.Value;
        _cierreService = cierreService;
        _movimientosService = movimientosService;
    }

    public async Task<ImportSummary> ImportAsync(string? filePath = null, bool dryRun = false)
    {
        filePath ??= _options.TreasuryExcelPath;
        if (!File.Exists(filePath))
        {
            var summary = new ImportSummary();
            summary.Errors.Add($"Archivo no encontrado: {filePath}");
            return summary;
        }
        
        using var stream = File.OpenRead(filePath);
        return await ImportAsync(stream, Path.GetFileName(filePath), dryRun);
    }

    public async Task<ImportSummary> ImportAsync(Stream excelStream, string sourceName, bool dryRun = false)
    {
        var summary = new ImportSummary();
        using var workbook = new XLWorkbook(excelStream);
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
            summary.Errors.Add("❌ FALLO CRÍTICO: No se encontraron hojas con formato reconocible. " +
                "Las hojas deben tener nombre como 'CORTE A OCTUBRE 31-25' o 'CORTE NOVIEMBRE 30-25'");
            summary.Success = false;
            return summary;
        }

        // ✅ VALIDACIÓN: Verificar que todas las hojas se identificaron correctamente
        var hojasNoIdentificadas = workbook.Worksheets
            .Where(s => !s.Name.Contains("RESUMEN") && !hojas.Any(h => h.sheet.Name == s.Name))
            .ToList();

        if (hojasNoIdentificadas.Count > 0)
        {
            summary.Errors.Add($"❌ FALLO CRÍTICO: {hojasNoIdentificadas.Count} hojas no tienen formato de fecha válido: {string.Join(", ", hojasNoIdentificadas.Select(h => h.Name))}");
            summary.Success = false;
            return summary;
        }

        // ✅ VALIDACIÓN CRÍTICA: Verificar que NINGÚN mes a importar esté cerrado
        var mesesCerrados = new List<string>();
        foreach (var (sheet, fecha) in hojas)
        {
            var esMesCerrado = await _cierreService.EsMesCerradoAsync(fecha.Year, fecha.Month);
            if (esMesCerrado)
            {
                mesesCerrados.Add($"{fecha:MMMM yyyy}");
            }
        }

        if (mesesCerrados.Count > 0)
        {
            summary.Errors.Add($"❌ BLOQUEO: No se puede importar. Los siguientes meses ya están CERRADOS: {string.Join(", ", mesesCerrados)}. " +
                $"Para re-importar, contacte al Admin para reabrir el período.");
            summary.Success = false;
            return summary;
        }

        foreach (var (sheet, fecha) in hojas)
        {
            var movimientos = ParseMovimientosFromSheet(sheet, cuenta.Id, fuentes, categorias, summary, sourceName);
            summary.MovimientosPorHoja[sheet.Name] = movimientos.Count;
            summary.PeriodoPorHoja[sheet.Name] = fecha.ToString("yyyy-MM");

            var saldoInicio = saldoAcumulado;
            summary.SaldoInicioPorHoja[sheet.Name] = saldoInicio;

            // 1) VALIDACIÓN DE ENTRADA: Comparar saldoMesAnterior con saldoAcumulado previo
            // Usa BalanceTolerancePolicy para validación centralizada
            if (summary.SaldoMesAnteriorPorHoja.TryGetValue(sheet.Name, out var saldoMesAnterior))
            {
                if (!BalanceTolerancePolicy.IsWithinTolerance(saldoMesAnterior.Value, saldoAcumulado))
                {
                    summary.BalanceMismatches++;
                    var context = $"Carry-over entre hojas - Hoja '{sheet.Name}' ({fecha:yyyy-MM})";
                    summary.Warnings.Add(BalanceTolerancePolicy.FormatMismatchMessage(context, saldoMesAnterior.Value, saldoAcumulado));
                }
            }

            // Procesar movimientos y calcular saldo acumulado
            foreach (var mov in movimientos)
            {
                // Calcular saldo esperado
                saldoAcumulado += mov.Tipo == TipoMovimientoTesoreria.Ingreso ? mov.Valor : -mov.Valor;
                
                // Verificar mismatch según BalanceTolerancePolicy (centralizado)
                if (mov.ImportBalanceExpected.HasValue)
                {
                    if (!BalanceTolerancePolicy.IsWithinTolerance(mov.ImportBalanceExpected.Value, saldoAcumulado))
                    {
                        mov.ImportHasBalanceMismatch = true;
                        mov.ImportBalanceFound = saldoAcumulado;
                        summary.BalanceMismatches++;
                        var context = $"Hoja '{sheet.Name}' ({fecha:yyyy-MM}), fila {mov.ImportRowNumber}";
                        summary.Warnings.Add(BalanceTolerancePolicy.FormatMismatchMessage(context, mov.ImportBalanceExpected.Value, saldoAcumulado));
                    }
                }
            }

            // 2) VALIDACIÓN DE SALIDA: Comparar saldoFinalEsperado con saldoAcumulado final
            // Usa BalanceTolerancePolicy para validación centralizada
            if (summary.SaldoFinalEsperadoPorHoja.TryGetValue(sheet.Name, out var saldoEsperado))
            {
                if (!BalanceTolerancePolicy.IsWithinTolerance(saldoEsperado.Value, saldoAcumulado))
                {
                    summary.BalanceMismatches++;
                    var context = $"Saldo fin de mes - Hoja '{sheet.Name}' ({fecha:yyyy-MM})";
                    summary.Warnings.Add(BalanceTolerancePolicy.FormatMismatchMessage(context, saldoEsperado.Value, saldoAcumulado));
                }
            }

            // Registrar saldo final calculado para auditoría por periodo
            summary.SaldoFinalCalculadoPorHoja[sheet.Name] = saldoAcumulado;

            // Importar movimientos con idempotencia usando servicio transaccional
            if (!dryRun)
            {
                var usuarioImport = "import-system";
                
                // ✅ OPTIMIZACIÓN CRÍTICA: Usar CreateManyAsync para batch import transaccional
                // Esto asegura:
                // 1. Validación de cierre contable por mes (eficiente)
                // 2. Idempotencia basada en ImportHash y NumeroMovimiento
                // 3. Una sola transacción para todos los movimientos
                // 4. Rollback automático si algo falla
                var (created, duplicates, closedErrors) = await _movimientosService.CreateManyAsync(movimientos, usuarioImport);

                // Actualizar summary con resultados
                summary.MovimientosImported += created.Count;
                summary.MovimientosSkipped += duplicates.Count;
                
                // Agregar errores de mes cerrado a summary
                foreach (var error in closedErrors)
                {
                    summary.Errors.Add(error);
                }
            }
            else
            {
                // En DryRun: reportar solo los que serían importados (no existentes)
                var existingHashes = (await db.MovimientosTesoreria
                    .Where(m => m.ImportHash != null)
                    .Select(m => m.ImportHash)
                    .ToListAsync())
                    .ToHashSet();

                var movimientosNuevos = movimientos
                    .Where(m => !existingHashes.Contains(m.ImportHash))
                    .Count();

                summary.MovimientosImported += movimientosNuevos;
                summary.MovimientosSkipped += movimientos.Count - movimientosNuevos;
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
    /// Lee movimientos de una hoja, detectando encabezado, excluyendo filas resumen, y capturando saldos
    /// </summary>
    private List<MovimientoTesoreria> ParseMovimientosFromSheet(
        IXLWorksheet sheet, 
        Guid cuentaId, 
        Dictionary<string, FuenteIngreso> fuentes,
        Dictionary<string, CategoriaEgreso> categorias,
        ImportSummary summary,
        string sourceName)
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
        decimal? saldoMesAnteriorEnHoja = null;
        decimal? saldoEnTesoreriaEnHoja = null;

        for (int r = startRow; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            var fechaCell = row.Cell(colFecha);
            var conceptoCell = row.Cell(colConcepto);
            var ingresosCell = row.Cell(colIngresos);
            var egresosCell = row.Cell(colEgresos);
            var saldoCell = colSaldo > 0 ? row.Cell(colSaldo) : null;

            // Detectar fila resumen y capturar saldos
            var concepto = conceptoCell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(concepto))
                continue;

            // Capturar SALDO EFECTIVO MES ANTERIOR (exacto o fallback)
            if (concepto.Contains("SALDO EFECTIVO MES ANTERIOR", StringComparison.OrdinalIgnoreCase) ||
                concepto.Contains("SALDO MES ANTERIOR", StringComparison.OrdinalIgnoreCase))
            {
                var val = saldoCell != null ? ParseDecimal(saldoCell) : ParseDecimal(ingresosCell);
                if (val != 0)
                    saldoMesAnteriorEnHoja = val;
                continue;
            }

            // Capturar SALDO EN TESORERIA (priorizar "A LA FECHA", luego fallback genérico)
            var isExactMatch = concepto.Contains("SALDO EN TESORERIA A LA FECHA", StringComparison.OrdinalIgnoreCase);
            var isFallbackMatch = !isExactMatch && concepto.Contains("SALDO EN TESORERIA", StringComparison.OrdinalIgnoreCase);
            
            if (isExactMatch || isFallbackMatch)
            {
                var val = saldoCell != null ? ParseDecimal(saldoCell) : ParseDecimal(ingresosCell);
                if (val != 0)
                    saldoEnTesoreriaEnHoja = val;
                continue;
            }

            // Descartar otras filas resumen
            if (IsResumenRow(concepto))
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
                ImportSource = sourceName,
                ImportSheet = sheet.Name,
                ImportRowNumber = r,
                ImportedAtUtc = DateTime.UtcNow,
                ImportBalanceExpected = saldo
            };

            movimientos.Add(mov);
        }

        // Registrar saldos detectados en la hoja
        if (saldoMesAnteriorEnHoja.HasValue)
            summary.SaldoMesAnteriorPorHoja[sheet.Name] = saldoMesAnteriorEnHoja.Value;
        if (saldoEnTesoreriaEnHoja.HasValue)
            summary.SaldoFinalEsperadoPorHoja[sheet.Name] = saldoEnTesoreriaEnHoja.Value;

        return movimientos;
    }

    /// <summary>
    /// Detecta si una fila es resumen (no debe importarse como movimiento).
    /// Usa frases específicas para evitar falsos positivos.
    /// </summary>
    private bool IsResumenRow(string concepto)
    {
        var upper = concepto.ToUpper();
        // Frases específicas de resumen
        var keywords = new[] { 
            "SALDO EFECTIVO MES ANTERIOR",
            "SALDO EN TESORERIA A LA FECHA",
            "SALDO EN TESORERIA",
            "TOTAL INGRESOS",
            "INGRESOS DOLARES",
            "TOTAL EGRESOS",
            "SALDO FINAL",
            "TOTAL DEPOSITOS"
        };
        return keywords.Any(k => upper.Contains(k));
    }

    /// <summary>
    /// Intenta parsear fecha desde celda (puede ser DateTime o texto), con soporte para es-CO
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
            
            // Intentar con CultureInfo es-CO primero
            var esCO = new CultureInfo("es-CO");
            if (DateTime.TryParse(str, esCO, System.Globalization.DateTimeStyles.None, out dt))
            {
                fecha = dt;
                return true;
            }
            
            // Fallback a parse invariante
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
    /// Calcula hash SHA256 para idempotencia (concepto normalizado: upper + colapsar espacios)
    /// </summary>
    private string ComputeHash(DateTime fecha, string concepto, TipoMovimientoTesoreria tipo, decimal valor, decimal? saldo, string sheet)
    {
        // Normalizar concepto: mayúsculas + colapsar espacios múltiples
        var conceptoNorm = System.Text.RegularExpressions.Regex.Replace(
            concepto.Trim().ToUpper(),
            @"\s+",
            " "
        );
        var data = $"{fecha:yyyy-MM-dd}|{conceptoNorm}|{tipo}|{valor}|{saldo}|{sheet}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes);
    }
}
