using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Deudores;
using Xunit;

namespace UnitTests;

/// <summary>
/// Pruebas unitarias para DeudoresService.
/// Valida cálculo de meses pendientes, filtros por rango, exclusión de meses pagados,
/// y casos edge (miembro sin FechaIngreso, miembro que ingresó después del rango, etc.).
/// </summary>
public class DeudoresServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CalcularAsync_MiembroAsociado_ExcluidoDeListado()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        db.Conceptos.Add(new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        });
        var asociado = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Anderson Arlex",
            Apellidos = "Betancur Rua",
            Documento = "1036634452",
            Email = "asociado@test.com",
            Telefono = "3000000008",
            Estado = EstadoMiembro.Activo,
            Rango = "Asociado",
            FechaIngreso = new DateOnly(2021, 10, 3)
        };
        db.Miembros.Add(asociado);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act
        var result = await svc.CalcularAsync(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 1));

        // Assert: no debe aparecer en el listado de deudores
        Assert.DoesNotContain(result, r => r.MiembroId == asociado.Id);
    }

    [Fact]
    public async Task CalcularAsync_MiembroSinFechaIngreso_DefaultPrimerMesDisponible()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembroSinIngreso = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Sin",
            Apellidos = "Fecha",
            Documento = "999",
            Email = "sin@test.com",
            Telefono = "3000000000",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = null // sin fecha de ingreso
        };
        db.Miembros.Add(miembroSinIngreso);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: sin rango
        var result = await svc.CalcularAsync(null, null);

        // Assert: debería tener al menos un mes pendiente (el mes actual)
        var row = result.FirstOrDefault(r => r.MiembroId == miembroSinIngreso.Id);
        Assert.NotNull(row);
        Assert.True(row.MesesPendientes.Count >= 1, "Miembro sin FechaIngreso debe tener al menos un mes pendiente.");
    }

    [Fact]
    public async Task CalcularAsync_ExcluyeMesesConPagos()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Con",
            Apellidos = "Pago",
            Documento = "111",
            Email = "pago@test.com",
            Telefono = "3000000001",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2024, 1, 1)
        };
        db.Miembros.Add(miembro);

        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);

        // Recibo pagado para enero 2024
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            Serie = "TST",
            Ano = 2024,
            Consecutivo = 1,
            MiembroId = miembro.Id,
            Estado = EstadoRecibo.Emitido,
            FechaEmision = new DateTime(2024, 1, 15),
            TotalCop = 20000
        };
        db.Recibos.Add(recibo);

        var item = new ReciboItem
        {
            Id = 1,
            ReciboId = recibo.Id,
            ConceptoId = concepto.Id,
            Cantidad = 1,
            PrecioUnitarioMonedaOrigen = 20000,
            SubtotalCop = 20000
        };
        db.ReciboItems.Add(item);

        var pago = new Pago
        {
            Id = 1,
            ReciboId = recibo.Id,
            FechaPago = new DateTime(2024, 1, 16),
            ValorPagadoCop = 20000,
            Metodo = MetodoPago.Transferencia
        };
        db.Pagos.Add(pago);

        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: calcular para rango ene-feb 2024
        var result = await svc.CalcularAsync(new DateOnly(2024, 1, 1), new DateOnly(2024, 2, 1));

        // Assert: enero pagado, febrero pendiente
        var row = result.FirstOrDefault(r => r.MiembroId == miembro.Id);
        Assert.NotNull(row);
        Assert.DoesNotContain(new DateOnly(2024, 1, 1), row.MesesPendientes);
        Assert.Contains(new DateOnly(2024, 2, 1), row.MesesPendientes);
    }

    [Fact]
    public async Task CalcularAsync_MultipleMesesAcumulados()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Multi",
            Apellidos = "Meses",
            Documento = "222",
            Email = "multi@test.com",
            Telefono = "3000000002",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2024, 1, 1)
        };
        db.Miembros.Add(miembro);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: calcular desde ene 2024 hasta jun 2024 (6 meses)
        var result = await svc.CalcularAsync(new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 1));

        // Assert: 6 meses pendientes (ene-jun)
        var row = result.FirstOrDefault(r => r.MiembroId == miembro.Id);
        Assert.NotNull(row);
        Assert.Equal(6, row.MesesPendientes.Count);
        Assert.Contains(new DateOnly(2024, 1, 1), row.MesesPendientes);
        Assert.Contains(new DateOnly(2024, 6, 1), row.MesesPendientes);
    }

    [Fact]
    public async Task CalcularAsync_RangoFiltrado_ExcluyeMesesFueraDeRango()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Rango",
            Apellidos = "Test",
            Documento = "333",
            Email = "rango@test.com",
            Telefono = "3000000003",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2020, 1, 1)
        };
        db.Miembros.Add(miembro);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: calcular solo mar-abr 2024
        var result = await svc.CalcularAsync(new DateOnly(2024, 3, 1), new DateOnly(2024, 4, 1));

        // Assert: solo 2 meses (mar, abr)
        var row = result.FirstOrDefault(r => r.MiembroId == miembro.Id);
        Assert.NotNull(row);
        Assert.Equal(2, row.MesesPendientes.Count);
        Assert.Contains(new DateOnly(2024, 3, 1), row.MesesPendientes);
        Assert.Contains(new DateOnly(2024, 4, 1), row.MesesPendientes);
        Assert.DoesNotContain(new DateOnly(2024, 2, 1), row.MesesPendientes);
    }

    [Fact]
    public async Task CalcularAsync_MiembroIngresoPostHasta_SinDeudas()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembroFuturo = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Futuro",
            Apellidos = "Miembro",
            Documento = "444",
            Email = "futuro@test.com",
            Telefono = "3000000004",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2025, 1, 1) // ingreso después del rango
        };
        db.Miembros.Add(miembroFuturo);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: calcular hasta dic 2024 (antes de ingreso)
        var result = await svc.CalcularAsync(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 1));

        // Assert: no debe aparecer en deudores (ingresó fuera del rango)
        var row = result.FirstOrDefault(r => r.MiembroId == miembroFuturo.Id);
        Assert.Null(row);
    }

    [Fact]
    public async Task CalcularAsync_MiembroInactivo_NoAparece()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembroInactivo = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Inactivo",
            Apellidos = "Test",
            Documento = "555",
            Email = "inactivo@test.com",
            Telefono = "3000000005",
            Estado = EstadoMiembro.Inactivo,
            FechaIngreso = new DateOnly(2024, 1, 1)
        };
        db.Miembros.Add(miembroInactivo);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act
        var result = await svc.CalcularAsync(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 1));

        // Assert: no debe aparecer (no está activo)
        var row = result.FirstOrDefault(r => r.MiembroId == miembroInactivo.Id);
        Assert.Null(row);
    }

    [Fact]
    public async Task CalcularAsync_SinRango_CalculaHastaHoy()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Sin",
            Apellidos = "Rango",
            Documento = "666",
            Email = "sinrango@test.com",
            Telefono = "3000000006",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2024, 10, 1)
        };
        db.Miembros.Add(miembro);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: sin desde/hasta (debería calcular hasta hoy)
        var result = await svc.CalcularAsync(null, null);

        // Assert: tiene meses pendientes desde oct 2024 hasta hoy (dependiendo de la fecha actual)
        var row = result.FirstOrDefault(r => r.MiembroId == miembro.Id);
        Assert.NotNull(row);
        Assert.True(row.MesesPendientes.Count > 0, "Debe tener al menos un mes pendiente hasta hoy.");
    }

    [Fact]
    public async Task CalcularAsync_RangoVacio_NoDeudas()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var concepto = new Concepto
        {
            Codigo = "MENSUALIDAD",
            Nombre = "Mensualidad",
            PrecioBase = 20000,
            Moneda = Moneda.COP,
            EsIngreso = true,
            EsRecurrente = true,
            Periodicidad = Periodicidad.Mensual
        };
        db.Conceptos.Add(concepto);
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            Nombres = "Rango",
            Apellidos = "Vacio",
            Documento = "777",
            Email = "vacio@test.com",
            Telefono = "3000000007",
            Estado = EstadoMiembro.Activo,
            FechaIngreso = new DateOnly(2024, 1, 1)
        };
        db.Miembros.Add(miembro);
        await db.SaveChangesAsync();

        var svc = new DeudoresService(db);

        // Act: desde > hasta (rango inválido/vacío)
        var result = await svc.CalcularAsync(new DateOnly(2024, 6, 1), new DateOnly(2024, 1, 1));

        // Assert: no debe haber deudores (rango inválido)
        var row = result.FirstOrDefault(r => r.MiembroId == miembro.Id);
        // Dependiendo de la implementación, puede retornar vacío o nulo
        if (row != null)
        {
            Assert.Empty(row.MesesPendientes);
        }
    }
}
