using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services.ImportHistorico;

/// <summary>
/// Servicio para importar histórico contable desde Excel "INFORME TESORERIA.xlsx"
/// con validación de saldos, idempotencia via ImportRowHash y evidencia auditable.
/// </summary>
public class ImportHistoricoService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ImportHistoricoService> _logger;

    public ImportHistoricoService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<ImportHistoricoService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el import (DRY-RUN o REAL) desde el Excel histórico.
    /// </summary>
    public async Task<ImportHistoricoResult> ExecuteImportAsync(string excelPath, bool dryRun, CancellationToken ct = default)
    {
        var result = new ImportHistoricoResult
        {
            DryRun = dryRun,
            StartTime = DateTime.UtcNow,
            ExcelPath = excelPath
        };

        try
        {
            // Calcular checksum del Excel
            result.ExcelSHA256 = CalculateSHA256(excelPath);
            _logger.LogInformation("Excel SHA256: {Hash}", result.ExcelSHA256);

            // Leer y parsear Excel
            var meses = ParseExcel(excelPath);
            _logger.LogInformation("Meses detectados en Excel: {Count}", meses.Count);

            // Validar cada mes
            foreach (var mes in meses)
            {
                ValidarSaldoMes(mes);
            }

            if (!dryRun)
            {
                // Import REAL: escribir en DB
                await ExecuteImportRealAsync(meses, result, ct);
            }
            else
            {
                // DRY-RUN: solo consultar duplicados sin escribir
                await ExecuteDryRunAsync(meses, result, ct);
            }

            result.Success = true;
            result.EndTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error durante import histórico");
        }

        return result;
    }

    private List<MesContable> ParseExcel(string excelPath)
    {
        var meses = new List<MesContable>();

        using var workbook = new XLWorkbook(excelPath);
        
        foreach (var worksheet in workbook.Worksheets)
        {
            var mes = ParseWorksheet(worksheet);
            if (mes != null)
            {
                meses.Add(mes);
            }
        }

        return meses.OrderBy(m => m.Anio).ThenBy(m => m.Mes).ToList();
    }

    private MesContable? ParseWorksheet(IXLWorksheet worksheet)
    {
        // Buscar título tipo "INFORME DE TESORERIA - CORTE <mes> <año>"
        var tituloRow = worksheet.RowsUsed().FirstOrDefault(r =>
            r.Cell(1).GetString().ToUpperInvariant().Contains("INFORME") &&
            r.Cell(1).GetString().ToUpperInvariant().Contains("TESORERIA"));

        if (tituloRow == null)
        {
            _logger.LogWarning("Hoja '{Name}' no tiene título de informe, se omite", worksheet.Name);
            return null;
        }

        var titulo = tituloRow.Cell(1).GetString();
        var (mes, anio) = ExtractMesAnioFromTitulo(titulo);
        
        _logger.LogInformation("Parseando hoja '{SheetName}': Título='{Titulo}', Mes={Mes}, Año={Anio}", 
            worksheet.Name, titulo, mes, anio);

        if (mes == 0 || anio == 0)
        {
            _logger.LogWarning("No se pudo extraer mes/año del título: {Titulo}", titulo);
            return null;
        }

        var mesContable = new MesContable
        {
            Mes = mes,
            Anio = anio,
            NombreMes = new DateTime(anio, mes, 1).ToString("MMMM yyyy", new CultureInfo("es-ES"))
        };

        // Buscar fila "SALDO EFECTIVO MES ANTERIOR"
        var saldoInicialRow = worksheet.RowsUsed().FirstOrDefault(r =>
            r.Cell(2).GetString().ToUpperInvariant().Contains("SALDO") &&
            r.Cell(2).GetString().ToUpperInvariant().Contains("ANTERIOR"));

        if (saldoInicialRow != null)
        {
            // Saldo está en columna SALDO (5)
            mesContable.SaldoInicial = ParseDecimal(saldoInicialRow.Cell(5).GetString());
        }

        // Leer movimientos desde la fila siguiente al saldo inicial hasta encontrar "TOTAL"
        int startRow = saldoInicialRow != null ? saldoInicialRow.RowNumber() + 1 : tituloRow.RowNumber() + 2;
        
        foreach (var row in worksheet.RowsUsed().Where(r => r.RowNumber() >= startRow))
        {
            var concepto = row.Cell(2).GetString().Trim();
            
            // Detener en TOTAL o líneas de resumen
            if (string.IsNullOrWhiteSpace(concepto) ||
                concepto.ToUpperInvariant().Contains("TOTAL") ||
                concepto.ToUpperInvariant().Contains("SALDO FINAL"))
            {
                break;
            }

            var fecha = ParseFecha(row.Cell(1).GetString(), mes, anio);
            var ingresoStr = row.Cell(3).GetString();
            var egresoStr = row.Cell(4).GetString();
            var saldoStr = row.Cell(5).GetString();

            var ingreso = ParseDecimal(ingresoStr);
            var egreso = ParseDecimal(egresoStr);
            var saldo = ParseDecimal(saldoStr);

            if (ingreso > 0)
            {
                mesContable.Ingresos.Add(new MovimientoIngreso
                {
                    Fecha = fecha,
                    Concepto = NormalizarConcepto(concepto),
                    Valor = ingreso,
                    Saldo = saldo
                });
            }
            else if (egreso > 0)
            {
                mesContable.Egresos.Add(new MovimientoEgreso
                {
                    Fecha = fecha,
                    Concepto = NormalizarConcepto(concepto),
                    Valor = egreso,
                    Saldo = saldo
                });
            }
        }

        _logger.LogInformation("Mes {Mes}/{Anio}: SaldoInicial={SaldoInicial:C}, Ingresos={Ingresos}, Egresos={Egresos}",
            mes, anio, mesContable.SaldoInicial, mesContable.Ingresos.Count, mesContable.Egresos.Count);

        return mesContable;
    }

    private (int mes, int anio) ExtractMesAnioFromTitulo(string titulo)
    {
        // Ejemplo: "INFORME DE TESORERIA - CORTE ENERO 2025" o "CORTE ENE 31 / 2025"
        var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"ENERO", 1}, {"ENE", 1}, {"FEBRERO", 2}, {"FEB", 2}, 
            {"MARZO", 3}, {"MAR", 3}, {"ABRIL", 4}, {"ABR", 4},
            {"MAYO", 5}, {"MAY", 5}, {"JUNIO", 6}, {"JUN", 6}, 
            {"JULIO", 7}, {"JUL", 7}, {"AGOSTO", 8}, {"AGO", 8},
            {"SEPTIEMBRE", 9}, {"SEP", 9}, {"OCTUBRE", 10}, {"OCT", 10}, 
            {"NOVIEMBRE", 11}, {"NOV", 11}, {"DICIEMBRE", 12}, {"DIC", 12}
        };

        int mes = 0, anio = 0;

        foreach (var (nombre, numero) in meses)
        {
            // Match con word boundary para evitar parciales (DIC no debe matchear DICIEMBRE)
            var pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(nombre)}\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(titulo.ToUpperInvariant(), pattern))
            {
                mes = numero;
                break;
            }
        }

        // Buscar año (4 dígitos consecutivos tipo 2025)
        // NOTA: Buscar último match para evitar capturar "20-25" como "20" (2020)
        var matches = System.Text.RegularExpressions.Regex.Matches(titulo, @"\b(20\d{2})\b");
        if (matches.Count > 0)
        {
            // Tomar el último match (suele ser el año completo "2025", no "20" del rango "20-25")
            anio = int.Parse(matches[matches.Count - 1].Groups[1].Value);
        }

        return (mes, anio);
    }

    private DateTime ParseFecha(string fechaStr, int mesPorDefecto, int anioPorDefecto)
    {
        if (string.IsNullOrWhiteSpace(fechaStr))
        {
            return new DateTime(anioPorDefecto, mesPorDefecto, 1);
        }

        // Intentar como DateTime directo
        if (DateTime.TryParse(fechaStr, out var fecha))
        {
            return fecha.Date;
        }

        // Intentar como número Excel (serial date)
        if (double.TryParse(fechaStr, out var serial))
        {
            return DateTime.FromOADate(serial).Date;
        }

        // Por defecto: primer día del mes
        return new DateTime(anioPorDefecto, mesPorDefecto, 1);
    }

    private decimal ParseDecimal(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return 0;

        // Quitar símbolos $ y separadores de miles (comas)
        // En Colombia: $1.234.567,89 → quitar $ y puntos, mantener coma como decimal
        valor = valor.Replace("$", "").Replace(".", "").Replace(",", ".").Trim();

        // Manejar paréntesis como negativos
        bool esNegativo = valor.StartsWith("(") && valor.EndsWith(")");
        if (esNegativo)
        {
            valor = valor.Trim('(', ')');
        }

        if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado))
        {
            return esNegativo ? -resultado : resultado;
        }

        return 0;
    }

    private string NormalizarConcepto(string concepto)
    {
        // Trim, espacios simples, uppercase
        concepto = concepto.Trim().ToUpperInvariant();
        concepto = System.Text.RegularExpressions.Regex.Replace(concepto, @"\s+", " ");
        return concepto;
    }

    private void ValidarSaldoMes(MesContable mes)
    {
        var totalIngresos = mes.Ingresos.Sum(i => i.Valor);
        var totalEgresos = mes.Egresos.Sum(e => e.Valor);
        var saldoCalculado = mes.SaldoInicial + totalIngresos - totalEgresos;

        // Saldo esperado es el saldo del último movimiento
        var ultimoMovimiento = mes.Ingresos.Cast<object>()
            .Concat(mes.Egresos.Cast<object>())
            .OrderByDescending(m => m is MovimientoIngreso i ? i.Fecha : ((MovimientoEgreso)m).Fecha)
            .FirstOrDefault();

        decimal saldoEsperado = ultimoMovimiento switch
        {
            MovimientoIngreso i => i.Saldo,
            MovimientoEgreso e => e.Saldo,
            _ => saldoCalculado
        };

        mes.TotalIngresos = totalIngresos;
        mes.TotalEgresos = totalEgresos;
        mes.SaldoCalculado = saldoCalculado;
        mes.SaldoEsperado = saldoEsperado;

        var diferencia = Math.Abs(saldoCalculado - saldoEsperado);
        if (diferencia > 0.5m)
        {
            _logger.LogWarning("⚠️  DISCREPANCIA CONTABLE en {Mes}: " +
                "SaldoCalculado={Calculado:C} vs SaldoEsperado={Esperado:C} (diferencia: {Diferencia:C}). " +
                "Import permitido, pero revisar Excel manualmente.",
                mes.NombreMes, saldoCalculado, saldoEsperado, diferencia);
            mes.ValidationOk = false;
        }
        else
        {
            _logger.LogInformation("✓ Validación OK {Mes}: Inicial={Inicial:C}, Ingresos={Ingresos:C}, Egresos={Egresos:C}, Final={Final:C}",
                mes.NombreMes, mes.SaldoInicial, totalIngresos, totalEgresos, saldoCalculado);
            mes.ValidationOk = true;
        }
    }

    private async Task ExecuteDryRunAsync(List<MesContable> meses, ImportHistoricoResult result, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        foreach (var mes in meses)
        {
            var mesResult = new MesImportResult
            {
                Mes = mes.Mes,
                Anio = mes.Anio,
                NombreMes = mes.NombreMes,
                SaldoInicial = mes.SaldoInicial,
                TotalIngresos = mes.TotalIngresos,
                TotalEgresos = mes.TotalEgresos,
                SaldoCalculado = mes.SaldoCalculado,
                SaldoEsperado = mes.SaldoEsperado
            };

            // Calcular hashes de todos los movimientos
            var hashesIngresos = mes.Ingresos.Select(i => CalculateIngresoHash(i, mes)).ToList();
            var hashesEgresos = mes.Egresos.Select(e => CalculateEgresoHash(e, mes)).ToList();

            // Consultar cuántos ya existen en DB
            var hashesExistentesIngresos = await context.Ingresos
                .Where(ing => hashesIngresos.Contains(ing.ImportRowHash!))
                .Select(ing => ing.ImportRowHash)
                .ToListAsync(ct);

            var hashesExistentesEgresos = await context.Egresos
                .Where(eg => hashesEgresos.Contains(eg.ImportRowHash!))
                .Select(eg => eg.ImportRowHash)
                .ToListAsync(ct);

            mesResult.IngresosLeidos = mes.Ingresos.Count;
            mesResult.IngresosDuplicados = hashesExistentesIngresos.Count;
            mesResult.IngresosNuevos = mesResult.IngresosLeidos - mesResult.IngresosDuplicados;

            mesResult.EgresosLeidos = mes.Egresos.Count;
            mesResult.EgresosDuplicados = hashesExistentesEgresos.Count;
            mesResult.EgresosNuevos = mesResult.EgresosLeidos - mesResult.EgresosDuplicados;

            result.MesesProcesados.Add(mesResult);

            _logger.LogInformation("DRY-RUN {Mes}: Ingresos nuevos={IngN}, duplicados={IngD}; Egresos nuevos={EgrN}, duplicados={EgrD}",
                mes.NombreMes, mesResult.IngresosNuevos, mesResult.IngresosDuplicados,
                mesResult.EgresosNuevos, mesResult.EgresosDuplicados);
        }
    }

    private async Task ExecuteImportRealAsync(List<MesContable> meses, ImportHistoricoResult result, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        foreach (var mes in meses)
        {
            var mesResult = new MesImportResult
            {
                Mes = mes.Mes,
                Anio = mes.Anio,
                NombreMes = mes.NombreMes,
                SaldoInicial = mes.SaldoInicial,
                TotalIngresos = mes.TotalIngresos,
                TotalEgresos = mes.TotalEgresos,
                SaldoCalculado = mes.SaldoCalculado,
                SaldoEsperado = mes.SaldoEsperado
            };

            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Insertar saldo inicial como registro especial (solo si no existe)
                var hashSaldoInicial = CalculateSaldoInicialHash(mes);
                var existeSaldoInicial = await context.Ingresos
                    .AnyAsync(i => i.ImportRowHash == hashSaldoInicial, ct);

                if (!existeSaldoInicial && mes.SaldoInicial != 0)
                {
                    var saldoInicialReg = new Ingreso
                    {
                        NumeroIngreso = $"SALDO-INICIAL-{mes.Anio:0000}-{mes.Mes:00}",
                        FechaIngreso = new DateTime(mes.Anio, mes.Mes, 1),
                        Categoria = "SaldoInicial",
                        Descripcion = $"SALDO INICIAL MES (IMPORT HISTORICO {mes.NombreMes.ToUpper()})",
                        ValorCop = mes.SaldoInicial,
                        MetodoPago = "N/A",
                        ImportRowHash = hashSaldoInicial,
                        CreatedBy = "import-historico",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Ingresos.Add(saldoInicialReg);
                }

                // 2. Insertar ingresos del mes
                mesResult.IngresosLeidos = mes.Ingresos.Count;
                int ingresosInsertados = 0, ingresosDuplicados = 0;

                foreach (var mov in mes.Ingresos)
                {
                    var hash = CalculateIngresoHash(mov, mes);
                    var existe = await context.Ingresos.AnyAsync(i => i.ImportRowHash == hash, ct);

                    if (!existe)
                    {
                        var ingreso = new Ingreso
                        {
                            NumeroIngreso = $"IMP-{mes.Anio:0000}{mes.Mes:00}-{Guid.NewGuid().ToString()[..8]}",
                            FechaIngreso = mov.Fecha,
                            Categoria = "Importado",
                            Descripcion = mov.Concepto,
                            ValorCop = mov.Valor,
                            MetodoPago = "Importado",
                            ImportRowHash = hash,
                            CreatedBy = "import-historico",
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Ingresos.Add(ingreso);
                        ingresosInsertados++;
                    }
                    else
                    {
                        ingresosDuplicados++;
                    }
                }

                mesResult.IngresosInsertados = ingresosInsertados;
                mesResult.IngresosDuplicados = ingresosDuplicados;
                mesResult.IngresosNuevos = ingresosInsertados;

                // 3. Insertar egresos del mes
                mesResult.EgresosLeidos = mes.Egresos.Count;
                int egresosInsertados = 0, egresosDuplicados = 0;

                foreach (var mov in mes.Egresos)
                {
                    var hash = CalculateEgresoHash(mov, mes);
                    var existe = await context.Egresos.AnyAsync(e => e.ImportRowHash == hash, ct);

                    if (!existe)
                    {
                        var egreso = new Egreso
                        {
                            Fecha = mov.Fecha,
                            Categoria = "Importado",
                            Proveedor = "Importado",
                            Descripcion = mov.Concepto,
                            ValorCop = mov.Valor,
                            UsuarioRegistro = "import-historico",
                            ImportRowHash = hash,
                            CreatedBy = "import-historico",
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Egresos.Add(egreso);
                        egresosInsertados++;
                    }
                    else
                    {
                        egresosDuplicados++;
                    }
                }

                mesResult.EgresosInsertados = egresosInsertados;
                mesResult.EgresosDuplicados = egresosDuplicados;
                mesResult.EgresosNuevos = egresosInsertados;

                // 4. Guardar cambios y commit
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                result.MesesProcesados.Add(mesResult);

                _logger.LogInformation("IMPORT REAL {Mes}: Ingresos insertados={IngI}, Egresos insertados={EgrI}",
                    mes.NombreMes, ingresosInsertados, egresosInsertados);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error importando mes {Mes}, rollback ejecutado", mes.NombreMes);
                throw;
            }
        }
    }

    private string CalculateIngresoHash(MovimientoIngreso mov, MesContable mes)
    {
        var data = $"Ingreso|{mov.Fecha:yyyy-MM-dd}|{mov.Valor:F2}|{mov.Concepto}|{mes.Anio:0000}-{mes.Mes:00}";
        return CalculateSHA256String(data);
    }

    private string CalculateEgresoHash(MovimientoEgreso mov, MesContable mes)
    {
        var data = $"Egreso|{mov.Fecha:yyyy-MM-dd}|{mov.Valor:F2}|{mov.Concepto}|{mes.Anio:0000}-{mes.Mes:00}";
        return CalculateSHA256String(data);
    }

    private string CalculateSaldoInicialHash(MesContable mes)
    {
        var data = $"SaldoInicial|{mes.Anio:0000}-{mes.Mes:00}|{mes.SaldoInicial:F2}";
        return CalculateSHA256String(data);
    }

    private string CalculateSHA256String(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private string CalculateSHA256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}

// Modelos auxiliares para el proceso de import
public class MesContable
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public List<MovimientoIngreso> Ingresos { get; set; } = new();
    public List<MovimientoEgreso> Egresos { get; set; } = new();
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoCalculado { get; set; }
    public decimal SaldoEsperado { get; set; }
    public bool ValidationOk { get; set; } = true; // Por defecto es válido
}

public class MovimientoIngreso
{
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Saldo { get; set; }
}

public class MovimientoEgreso
{
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Saldo { get; set; }
}

public class ImportHistoricoResult
{
    public bool DryRun { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ExcelPath { get; set; } = string.Empty;
    public string ExcelSHA256 { get; set; } = string.Empty;
    public List<MesImportResult> MesesProcesados { get; set; } = new();

    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
}

public class MesImportResult
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoCalculado { get; set; }
    public decimal SaldoEsperado { get; set; }
    
    public int IngresosLeidos { get; set; }
    public int IngresosNuevos { get; set; }
    public int IngresosInsertados { get; set; }
    public int IngresosDuplicados { get; set; }
    
    public int EgresosLeidos { get; set; }
    public int EgresosNuevos { get; set; }
    public int EgresosInsertados { get; set; }
    public int EgresosDuplicados { get; set; }

    public bool ValidationOk => Math.Abs(SaldoCalculado - SaldoEsperado) <= 0.5m;
}
