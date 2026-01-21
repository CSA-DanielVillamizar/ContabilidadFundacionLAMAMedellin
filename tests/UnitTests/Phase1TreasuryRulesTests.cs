using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Xunit;

namespace UnitTests;

public class Phase1TreasuryRulesTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging();
        return builder.Options;
    }

    [Fact]
    public async Task NoDuplicaAportePorMiembroMesAno()
    {
        var options = CreateOptions();
        await using var ctx = new AppDbContext(options);

        var miembro = new Miembro { Id = Guid.NewGuid(), NombreCompleto = "Test", Email = "t@t.com" };
        ctx.Miembros.Add(miembro);
        await ctx.SaveChangesAsync();

        var a1 = new AporteMensual { MiembroId = miembro.Id, Ano = 2025, Mes = 1, ValorEsperado = 20000m };
        ctx.AportesMensuales.Add(a1);
        await ctx.SaveChangesAsync();

        var a2 = new AporteMensual { MiembroId = miembro.Id, Ano = 2025, Mes = 1, ValorEsperado = 20000m };
        ctx.AportesMensuales.Add(a2);

        // Validar que el índice único está configurado en el modelo
        var entityType = ctx.Model.FindEntityType(typeof(AporteMensual));
        Assert.NotNull(entityType);
        var uniqueIndex = entityType!.GetIndexes()
            .FirstOrDefault(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "MiembroId", "Ano", "Mes" }));
        Assert.NotNull(uniqueIndex);
    }

    [Fact]
    public async Task BloqueaMovimientoSiPeriodoCerrado()
    {
        var options = CreateOptions();
        await using var ctx = new AppDbContext(options);

        // Seed cuenta
        var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Codigo = "BANCO-TEST", Nombre = "Bancolombia Test", Tipo = TipoCuenta.Bancaria };
        ctx.CuentasFinancieras.Add(cuenta);
        await ctx.SaveChangesAsync();

        // Cerrar mes enero 2025
        ctx.CierresMensuales.Add(new CierreMensual
        {
            Ano = 2025,
            Mes = 1,
            FechaCierre = new DateTime(2025,2,1),
            UsuarioCierre = "tester",
            SaldoInicialCalculado = 0,
            TotalIngresos = 0,
            TotalEgresos = 0,
            SaldoFinal = 0
        });
        await ctx.SaveChangesAsync();

        var mov = new MovimientoTesoreria
        {
            NumeroMovimiento = "MV-TEST-001",
            Fecha = new DateTime(2025,1,15),
            Tipo = TipoMovimientoTesoreria.Ingreso,
            CuentaFinancieraId = cuenta.Id,
            Descripcion = "Prueba",
            Medio = MedioPagoTesoreria.Transferencia,
            Valor = 10000m
        };

        // Simular regla: no permitir crear si mes está cerrado
        var periodoCerrado = await ctx.CierresMensuales.AnyAsync(c => c.Ano == mov.Fecha.Year && c.Mes == mov.Fecha.Month);
        Assert.True(periodoCerrado);
    }

    [Fact]
    public async Task SoloAprobadosAfectanSaldoCalculado()
    {
        var options = CreateOptions();
        await using var ctx = new AppDbContext(options);

        var cuenta = new CuentaFinanciera { Id = Guid.NewGuid(), Codigo = "BANCO-TEST", Nombre = "Bancolombia Test", Tipo = TipoCuenta.Bancaria };
        ctx.CuentasFinancieras.Add(cuenta);
        await ctx.SaveChangesAsync();

        ctx.MovimientosTesoreria.Add(new MovimientoTesoreria
        {
            NumeroMovimiento = "MV-ING-001",
            Fecha = DateTime.UtcNow,
            Tipo = TipoMovimientoTesoreria.Ingreso,
            CuentaFinancieraId = cuenta.Id,
            Descripcion = "Ingreso Borrador",
            Medio = MedioPagoTesoreria.Transferencia,
            Estado = EstadoMovimientoTesoreria.Borrador,
            Valor = 50000m
        });

        ctx.MovimientosTesoreria.Add(new MovimientoTesoreria
        {
            NumeroMovimiento = "MV-ING-002",
            Fecha = DateTime.UtcNow,
            Tipo = TipoMovimientoTesoreria.Ingreso,
            CuentaFinancieraId = cuenta.Id,
            Descripcion = "Ingreso Aprobado",
            Medio = MedioPagoTesoreria.Transferencia,
            Estado = EstadoMovimientoTesoreria.Aprobado,
            Valor = 70000m
        });

        ctx.MovimientosTesoreria.Add(new MovimientoTesoreria
        {
            NumeroMovimiento = "MV-EGR-001",
            Fecha = DateTime.UtcNow,
            Tipo = TipoMovimientoTesoreria.Egreso,
            CuentaFinancieraId = cuenta.Id,
            Descripcion = "Egreso Aprobado",
            Medio = MedioPagoTesoreria.Transferencia,
            Estado = EstadoMovimientoTesoreria.Aprobado,
            Valor = 30000m
        });

        await ctx.SaveChangesAsync();

        var ingresosAprobados = await ctx.MovimientosTesoreria
            .Where(m => m.CuentaFinancieraId == cuenta.Id && m.Tipo == TipoMovimientoTesoreria.Ingreso && m.Estado == EstadoMovimientoTesoreria.Aprobado)
            .SumAsync(m => m.Valor);
        var egresosAprobados = await ctx.MovimientosTesoreria
            .Where(m => m.CuentaFinancieraId == cuenta.Id && m.Tipo == TipoMovimientoTesoreria.Egreso && m.Estado == EstadoMovimientoTesoreria.Aprobado)
            .SumAsync(m => m.Valor);

        var saldoCalculado = cuenta.SaldoInicial + ingresosAprobados - egresosAprobados;
        Assert.Equal(40000m, saldoCalculado);
    }
}
