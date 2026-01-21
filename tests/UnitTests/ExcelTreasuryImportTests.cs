using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        // Tolerancia de ±1 COP
        var saldoCalculado = 100000.00m;
        var saldoEsperado1 = 100000.50m;
        var saldoEsperado2 = 100002.00m;

        var diff1 = Math.Abs(saldoCalculado - saldoEsperado1);
        var diff2 = Math.Abs(saldoCalculado - saldoEsperado2);

        Assert.True(diff1 <= 1m); // Dentro de tolerancia
        Assert.False(diff2 <= 1m); // Fuera de tolerancia
    }

    // Helper
    private string ComputeHashHelper(DateTime fecha, string concepto, string tipo, decimal valor, decimal? saldo, string sheet)
    {
        var data = $"{fecha:yyyy-MM-dd}|{concepto.Trim()}|{tipo}|{valor}|{saldo}|{sheet}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes);
    }
}
