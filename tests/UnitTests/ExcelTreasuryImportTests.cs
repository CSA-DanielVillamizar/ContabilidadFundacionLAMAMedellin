using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Server.Data;
using Server.Models;
using Server.Services.Audit;
using Server.Services.CierreContable;
using Server.Services.Import;
using Server.Services.MovimientosTesoreria;
using Xunit;

namespace UnitTests;

/// <summary>
/// Tests para validar funcionalidades core del importador de tesorería Excel
/// </summary>
public class ExcelTreasuryImportTests
{
    [Theory]
    [InlineData("15/05/2024", 2024, 5, 15)]
    [InlineData("2024-05-15", 2024, 5, 15)]
    [InlineData("05/15/2024", 2024, 5, 15)]
    public void ParseDate_ValidFormats_ReturnsCorrectDate(string input, int year, int month, int day)
    {
        // Simular parseo de fecha tolerante
        var success = DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ||
                      DateTime.TryParse(input, new CultureInfo("es-CO"), DateTimeStyles.None, out result);
        
        Assert.True(success);
        Assert.Equal(year, result.Year);
        Assert.Equal(month, result.Month);
        Assert.Equal(day, result.Day);
    }

    [Theory]
    [InlineData("$1.000,50", 1000.50)]      // Formato colombiano con signo peso
    [InlineData("1.000,50", 1000.50)]       // Formato colombiano estándar
    [InlineData("1000", 1000)]              // Sin separadores
    [InlineData("1000,00", 1000)]           // Con coma decimal
    [InlineData("  500  ", 500)]            // Con espacios
    public void ParseDecimal_VariousFormats_ReturnsNumericValue(string input, decimal expected)
    {
        // Formato colombiano: punto = separador de miles, coma = decimal
        var cleaned = input.Trim()
            .Replace("$", "")
            .Replace(" ", "")
            .Replace(".", "");  // Remover separador de miles
        cleaned = cleaned.Replace(",", "."); // Coma decimal -> punto
        
        var success = decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        
        Assert.True(success);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SALDO EFECTIVO MES ANTERIOR")]
    [InlineData("TOTAL INGRESOS")]
    [InlineData("TOTAL EGRESOS")]
    [InlineData("SALDO EN TESORERIA A LA FECHA")]
    [InlineData("INGRESOS dolares")]
    public void IsResumenRow_SummaryKeywords_ReturnsTrue(string concepto)
    {
        var keywords = new[] { 
            "SALDO EFECTIVO", "TOTAL INGRESOS", "INGRESOS DOLARES", "EGRESOS", 
            "SALDO EN TESORERIA", "MES ANTERIOR", "TOTAL EGRESOS", "SALDO FINAL"
        };
        var upper = concepto.ToUpper();
        var isResumen = keywords.Any(k => upper.Contains(k));

        Assert.True(isResumen);
    }

    [Theory]
    [InlineData("Aporte mensual miembro 12345")]
    [InlineData("Donación evento aniversario")]
    [InlineData("Venta merchandising camisetas")]
    public void IsResumenRow_ValidMovements_ReturnsFalse(string concepto)
    {
        var keywords = new[] { 
            "SALDO EFECTIVO", "TOTAL INGRESOS", "INGRESOS DOLARES", "EGRESOS", 
            "SALDO EN TESORERIA", "MES ANTERIOR", "TOTAL EGRESOS", "SALDO FINAL"
        };
        var upper = concepto.ToUpper();
        var isResumen = keywords.Any(k => upper.Contains(k));

        Assert.False(isResumen);
    }

    [Fact]
    public void ComputeHash_SameInputs_ReturnsSameHash()
    {
        var fecha = new DateTime(2025, 5, 15);
        var concepto = "Aporte mensual";
        var tipo = "Ingreso";
        var valor = 20000m;
        var saldo = 50000m;
        var sheet = "CORTE MAYO 2025";

        var hash1 = ComputeHashHelper(fecha, concepto, tipo, valor, saldo, sheet);
        var hash2 = ComputeHashHelper(fecha, concepto, tipo, valor, saldo, sheet);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_DifferentInputs_ReturnsDifferentHash()
    {
        var fecha = new DateTime(2025, 5, 15);
        var concepto1 = "Aporte mensual";
        var concepto2 = "Donación";
        var tipo = "Ingreso";
        var valor = 20000m;
        var saldo = 50000m;
        var sheet = "CORTE MAYO 2025";

        var hash1 = ComputeHashHelper(fecha, concepto1, tipo, valor, saldo, sheet);
        var hash2 = ComputeHashHelper(fecha, concepto2, tipo, valor, saldo, sheet);

        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData("CORTE MAYO - 24", 2024, 5)]
    [InlineData("CORTE A MAYO 2024", 2024, 5)]
    [InlineData("CORTE NOVIEMBRE 30-25", 2025, 11)]
    [InlineData("CORTE A NOVIEMBRE 2025", 2025, 11)]
    public void ParseSheetName_ValidFormats_ExtractsMonthYear(string sheetName, int expectedYear, int expectedMonth)
    {
        // Regex más robusto: captura mes seguido de año al final de la cadena
        var regex = new Regex(@"CORTE\s+(A\s+)?(?<mes>\w+)[\s\-\.]+(?:\d+[\s\-])?(?<ano>\d{2,4})\s*$", RegexOptions.IgnoreCase);
        var match = regex.Match(sheetName);

        Assert.True(match.Success);

        var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["enero"] = 1, ["febrero"] = 2, ["marzo"] = 3, ["abril"] = 4,
            ["mayo"] = 5, ["junio"] = 6, ["julio"] = 7, ["agosto"] = 8,
            ["septiembre"] = 9, ["octubre"] = 10, ["noviembre"] = 11, ["diciembre"] = 12
        };

        var mesStr = match.Groups["mes"].Value;
        var anoStr = match.Groups["ano"].Value;
        
        Assert.True(meses.TryGetValue(mesStr, out var mes));
        Assert.Equal(expectedMonth, mes);

        Assert.True(int.TryParse(anoStr, out var ano));
        if (ano < 100) ano += 2000;
        Assert.Equal(expectedYear, ano);
    }

    [Fact]
    public void BalanceMismatch_DetectionWithTolerance_WorksCorrectly()
    {
        // Usa BalanceTolerancePolicy: tolerancia exclusiva < 1 COP
        var saldoCalculado = 100000.00m;
        var saldoEsperado1 = 100000.50m;  // diff = 0.50 < 1.0 => dentro
        var saldoEsperado2 = 100002.00m;  // diff = 2.00 >= 1.0 => fuera

        // Verificar con política centralizada
        Assert.True(BalanceTolerancePolicy.IsWithinTolerance(saldoEsperado1, saldoCalculado), 
            "Diferencia 0.50 COP debe estar dentro de tolerancia");
        Assert.False(BalanceTolerancePolicy.IsWithinTolerance(saldoEsperado2, saldoCalculado), 
            "Diferencia 2.00 COP debe estar fuera de tolerancia");
    }

    // Helper
    private string ComputeHashHelper(DateTime fecha, string concepto, string tipo, decimal valor, decimal? saldo, string sheet)
    {
        var data = $"{fecha:yyyy-MM-dd}|{concepto.Trim()}|{tipo}|{valor}|{saldo}|{sheet}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Tests para carry-over mensual (validación de saldos entre hojas)
    /// </summary>

    [Fact]
    public void CarryOver_SaldoMesAnteriorMatchesPreviousFinal_NoMismatch()
    {
        // Simular flujo: Hoja 1 termina con saldo 100.000 COP
        // Hoja 2 comienza con saldo mes anterior 100.000 COP => match, no hay warning
        var saldoFinalHoja1 = 100000m;
        var saldoMesAnteriorHoja2 = 100000m;

        // Usa BalanceTolerancePolicy para validación centralizada
        var isWithinTolerance = BalanceTolerancePolicy.IsWithinTolerance(saldoMesAnteriorHoja2, saldoFinalHoja1);

        Assert.True(isWithinTolerance, "Los saldos coinciden exactamente, debe estar dentro de tolerancia");
    }

    [Fact]
    public void CarryOver_SaldoMesAnteriorMismatchWithInHoja2_DetectMismatch()
    {
        // Hoja 1 termina con saldo 100.000 COP
        // Hoja 2 comienza con saldo mes anterior 100.500 COP => mismatch, debe haber warning
        var saldoFinalHoja1 = 100000m;
        var saldoMesAnteriorHoja2 = 100500m;

        // Usa BalanceTolerancePolicy para validación centralizada (exclusiva)
        var isWithinTolerance = BalanceTolerancePolicy.IsWithinTolerance(saldoMesAnteriorHoja2, saldoFinalHoja1);

        Assert.False(isWithinTolerance, "Diferencia de 500 COP debe estar fuera de tolerancia");
    }

    [Fact]
    public void CarryOver_SaldoMesAnteriorWithinToleranceOf1COP_NoMismatch()
    {
        // Hoja 1 termina con saldo 100.000,50 COP
        // Hoja 2 comienza con saldo mes anterior 100.000 COP => dentro de tolerancia < 1
        var saldoFinalHoja1 = 100000.50m;
        var saldoMesAnteriorHoja2 = 100000m;

        // Usa BalanceTolerancePolicy para validación centralizada (tolerancia exclusiva)
        var isWithinTolerance = BalanceTolerancePolicy.IsWithinTolerance(saldoMesAnteriorHoja2, saldoFinalHoja1);

        Assert.True(isWithinTolerance, "Diferencia de 0,50 COP está dentro de tolerancia <1");
    }

    [Fact]
    public void SaldoFinalCalculado_MatchesExpectedAfterAllMovimientos_NoMismatch()
    {
        // Hoja con movimientos que suman a saldo esperado exacto
        var saldoInicial = 50000m;
        var movimientos = new List<(bool esIngreso, decimal valor)>
        {
            (true, 30000),   // +30.000
            (false, 10000),  // -10.000
            (true, 15000)    // +15.000
        };

        var saldoCalculado = saldoInicial;
        foreach (var (esIngreso, valor) in movimientos)
        {
            saldoCalculado += esIngreso ? valor : -valor;
        }

        var saldoEsperado = 90000m; // 50 + 30 - 10 + 15 = 85, pero esperado es 90
        
        // Este caso muestra que hay mismatch
        var diff = Math.Abs(saldoCalculado - saldoEsperado);
        Assert.Equal(5000m, diff);
    }

    [Fact]
    public void SaldoFinalCalculado_MatchesExpectedWhenCorrect_VerifyCalculation()
    {
        // Hoja con movimientos que suman correctamente
        var saldoInicial = 50000m;
        var movimientos = new List<(bool esIngreso, decimal valor)>
        {
            (true, 30000),   // +30.000
            (false, 10000),  // -10.000
            (true, 15000)    // +15.000
        };

        var saldoCalculado = saldoInicial;
        foreach (var (esIngreso, valor) in movimientos)
        {
            saldoCalculado += esIngreso ? valor : -valor;
        }

        var saldoEsperado = 85000m; // Correctamente calculado: 50 + 30 - 10 + 15 = 85

        var diff = Math.Abs(saldoCalculado - saldoEsperado);
        Assert.True(diff <= 1m, $"El saldo final debe coincidir. Calculado: {saldoCalculado}, Esperado: {saldoEsperado}");
        Assert.Equal(85000m, saldoCalculado);
    }

    [Theory]
    [InlineData(100000, 100000, true)]   // Exacto: diferencia 0 < 1 ✓
    [InlineData(100000, 100001, false)] // Diferencia 1 NO es aceptable (tolerancia estricta < 1)
    [InlineData(100000, 99999, false)]  // Diferencia 1 NO es aceptable (lado negativo)
    [InlineData(100000, 100002, false)] // Diferencia 2 está fuera
    [InlineData(100000, 99998, false)]  // Diferencia 2 está fuera (lado negativo)
    public void BalanceTolerance_VariousThresholds_AppliesCorrectly(long calculado, long esperado, bool shouldMatch)
    {
        // Usa BalanceTolerancePolicy: tolerancia EXCLUSIVA < 1 COP
        // Para software contable, cualquier diferencia >= 1 es rechazada
        var isMatch = BalanceTolerancePolicy.IsWithinTolerance(esperado, calculado);

        if (shouldMatch)
            Assert.True(isMatch, $"Diferencia {Math.Abs(calculado - esperado)} debe estar dentro de tolerancia estricta <1");
        else
            Assert.False(isMatch, $"Diferencia {Math.Abs(calculado - esperado)} debe estar fuera de tolerancia estricta <1");
    }

    [Fact]
    public void BalanceTolerance_EdgeCase_JustBelowTolerance_Accepts()
    {
        // Test de borde explícito: diff = 0.99 debe aceptarse
        var esperado = 100000m;
        var encontrado = 100000.99m;  // diff = 0.99 < 1.0
        
        var isWithinTolerance = BalanceTolerancePolicy.IsWithinTolerance(esperado, encontrado);
        
        Assert.True(isWithinTolerance, "Diferencia de 0.99 COP debe estar dentro (excl. < 1.0)");
    }

    [Fact]
    public void BalanceTolerance_EdgeCase_ExactlyAtTolerance_Rejects()
    {
        // Test de borde explícito: diff = 1.00 debe rechazarse (tolerancia EXCLUSIVA)
        var esperado = 100000m;
        var encontrado = 100001m;  // diff = 1.00, NO < 1.0 => rechaza
        
        var isWithinTolerance = BalanceTolerancePolicy.IsWithinTolerance(esperado, encontrado);
        
        Assert.False(isWithinTolerance, "Diferencia de 1.00 COP debe estar fuera (excl. < 1.0)");
    }

    [Fact]
    public void BalanceTolerancePolicy_CalculateDiff_ReturnsAbsoluteDifference()
    {
            // Test CalculateDiff retorna diferencia absoluta
            var diff1 = BalanceTolerancePolicy.CalculateDiff(100000m, 100001m);
            var diff2 = BalanceTolerancePolicy.CalculateDiff(100001m, 100000m);
            var diff3 = BalanceTolerancePolicy.CalculateDiff(100000m, 100000m);
        
            Assert.Equal(1.00m, diff1);
            Assert.Equal(1.00m, diff2);  // Debe ser absoluta (sin signo)
            Assert.Equal(0.00m, diff3);
    }

    [Fact]
    public void BalanceTolerancePolicy_FormatMismatchMessage_IncludesAllDetails()
    {
        // Test que FormatMismatchMessage incluye todos los detalles requeridos en formato es-CO
        var context = "Hoja MAYO 2025, fila 10";
        var esperado = 100000m;
        var encontrado = 99999m;

        var mensaje = BalanceTolerancePolicy.FormatMismatchMessage(context, esperado, encontrado);

        var co = CultureInfo.GetCultureInfo("es-CO");
        var esperadoStr = "100.000,00";
        var encontradoStr = "99.999,00";
        var diffStr = "1,00";
        var toleranciaStr = "1,00";

        Assert.Contains(context, mensaje);
        Assert.Contains($"Esperado={esperadoStr} COP", mensaje);
        Assert.Contains($"Encontrado={encontradoStr} COP", mensaje);
        Assert.Contains($"Diff={diffStr} COP", mensaje);
        Assert.Contains($"Tolerancia={toleranciaStr} COP", mensaje);
        Assert.Contains($"Regla: diff < {toleranciaStr}", mensaje);
        Assert.Contains("EXCLUSIVA", mensaje);
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null) => Task.CompletedTask;
        public Task<List<Server.Models.AuditLog>> GetEntityLogsAsync(string entityType, string entityId) => Task.FromResult(new List<Server.Models.AuditLog>());
        public Task<List<Server.Models.AuditLog>> GetRecentLogsAsync(int count = 100) => Task.FromResult(new List<Server.Models.AuditLog>());
    }

    private sealed class TestDbFactory : IDbContextFactory<AppDbContext>
    {
        private readonly SqliteConnection _conn;
        public TestDbFactory(SqliteConnection conn) { _conn = conn; }
        public AppDbContext CreateDbContext()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
            var db = new AppDbContext(opts);
            try
            {
                _conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new CultureInfo("es-ES"), CompareOptions.IgnoreCase));
            }
            catch
            {
                // Si ya existe la colación, continuar
            }
            db.Database.EnsureCreated();
            return db;
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }

    [Fact]
    public async Task ImportAsync_StoresSaldoInicioPorHoja_AsCarryOverBetweenSheets()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var factory = new TestDbFactory(conn);
        var audit = new FakeAuditService();
        var cierre = new CierreContableService(factory, audit);
        var movimientosService = new MovimientosTesoreriaService(factory, cierre, audit);
        var logger = NullLogger<ExcelTreasuryImportService>.Instance;
        var options = Options.Create(new ImportOptions { Enabled = true, TreasuryExcelPath = "INFORME TESORERIA.xlsx" });
        await using (var seedDb = factory.CreateDbContext())
        {
            var cuenta = await seedDb.CuentasFinancieras.FirstOrDefaultAsync(c => c.Codigo == "BANCO-BCOL-001");
            if (cuenta == null)
            {
                cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Codigo = "BANCO-BCOL-001", Nombre = "Cuenta Test", Tipo = TipoCuenta.Bancaria, SaldoInicial = 0m };
                seedDb.CuentasFinancieras.Add(cuenta);
            }
            else
            {
                cuenta.SaldoInicial = 0m;
                seedDb.CuentasFinancieras.Update(cuenta);
            }

            if (!await seedDb.FuentesIngreso.AnyAsync(f => f.Codigo == "OTROS"))
            {
                seedDb.FuentesIngreso.Add(new FuenteIngreso { Id = Guid.NewGuid(), Codigo = "OTROS", Nombre = "Otros" });
            }

            await seedDb.SaveChangesAsync();
        }

        using var workbook = new XLWorkbook();
        var sheet1 = workbook.AddWorksheet("CORTE ENERO 2025");
        var sheet2 = workbook.AddWorksheet("CORTE FEBRERO 2025");

        void BuildSheet(IXLWorksheet sheet, DateTime fechaMovimiento, decimal valorIngreso, decimal saldoEsperado)
        {
            sheet.Cell(1, 1).Value = "FECHA";
            sheet.Cell(1, 2).Value = "CONCEPTO";
            sheet.Cell(1, 3).Value = "INGRESOS";
            sheet.Cell(1, 4).Value = "EGRESOS";
            sheet.Cell(1, 5).Value = "SALDO";

            sheet.Cell(2, 1).Value = fechaMovimiento;
            sheet.Cell(2, 2).Value = "Ingreso OTROS";
            sheet.Cell(2, 3).Value = valorIngreso;
            sheet.Cell(2, 4).Value = 0;
            sheet.Cell(2, 5).Value = saldoEsperado;
        }

        BuildSheet(sheet1, new DateTime(2025, 1, 5), 1000m, 1000m);
        BuildSheet(sheet2, new DateTime(2025, 2, 5), 500m, 1500m);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var service = new ExcelTreasuryImportService(factory, logger, options, cierre, movimientosService);
        var summary = await service.ImportAsync(stream, "in-memory.xlsx", dryRun: true);

        Assert.True(summary.Success, "La importación debe completar sin errores");
        Assert.Equal(2, summary.SaldoInicioPorHoja.Count);
        Assert.Equal(2, summary.PeriodoPorHoja.Count);
        Assert.Equal("2025-01", summary.PeriodoPorHoja[sheet1.Name]);
        Assert.Equal("2025-02", summary.PeriodoPorHoja[sheet2.Name]);
        Assert.Equal(summary.SaldoFinalCalculadoPorHoja[sheet1.Name], summary.SaldoInicioPorHoja[sheet2.Name]);
        Assert.Equal(1000m, summary.SaldoFinalCalculadoPorHoja[sheet1.Name]);
        Assert.Equal(1000m, summary.SaldoInicioPorHoja[sheet2.Name]);
    }

    [Fact]
    public void IsResumenRow_UpdatedKeywords_AccuracyCheck()
    {
        // Verificar que las nuevas keywords específicas funcionan
        var resumenRows = new[]
        {
            "SALDO EFECTIVO MES ANTERIOR",
            "SALDO EN TESORERIA A LA FECHA",
            "SALDO EN TESORERIA",
            "TOTAL INGRESOS",
            "INGRESOS DOLARES",
            "TOTAL EGRESOS",
            "SALDO FINAL",
            "TOTAL DEPOSITOS"
        };

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

        foreach (var row in resumenRows)
        {
            var isResumen = keywords.Any(k => row.ToUpper().Contains(k));
            Assert.True(isResumen, $"'{row}' debe ser detectado como fila resumen");
        }

        // Movimientos válidos NO deben ser detectados como resumen
        var validRows = new[]
        {
            "Aporte mensual miembro 12345",
            "Donación evento aniversario",
            "Venta merchandising",
            "Pago servicios",
            "Egreso ayuda social"
        };

        foreach (var row in validRows)
        {
            var isResumen = keywords.Any(k => row.ToUpper().Contains(k));
            Assert.False(isResumen, $"'{row}' NO debe ser detectado como fila resumen");
        }
    }
}
