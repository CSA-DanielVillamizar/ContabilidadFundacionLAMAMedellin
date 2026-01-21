using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Server.Data;
using Server.Models;
using Server.Services.Audit;
using Server.Services.CierreContable;

namespace UnitTests;

/// <summary>
/// Tests para validar funcionalidades del servicio de cierre contable mensual
/// Verifica integridad, auditoría, validaciones y bloqueos
/// </summary>
public class CierreContableServiceTests
{
    /// <summary>
    /// Crea un contexto en memoria para pruebas aisladas
    /// </summary>
    private static async Task<(AppDbContext Context, CierreContableService Service)> CreateTestContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_cierre_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var auditService = new MockAuditService();
        var factory = new TestDbContextFactory(context);
        var service = new CierreContableService(factory, auditService);

        return (context, service);
    }

    [Fact]
    public async Task EsMesCerradoAsync_MesCerrado_ReturnsTrue()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        var cierre = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = 2025,
            Mes = 5,
            FechaCierre = DateTime.Now,
            UsuarioCierre = "admin@test.com",
            SaldoInicialCalculado = 100000m,
            TotalIngresos = 50000m,
            TotalEgresos = 30000m,
            SaldoFinal = 120000m,
            CreatedAt = DateTime.Now,
            CreatedBy = "admin@test.com"
        };
        context.CierresMensuales.Add(cierre);
        await context.SaveChangesAsync();

        // Act
        var resultado = await service.EsMesCerradoAsync(2025, 5);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task EsMesCerradoAsync_MesAbierto_ReturnsFalse()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        // No agregar ningún cierre

        // Act
        var resultado = await service.EsMesCerradoAsync(2025, 5);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task EsFechaCerradaAsync_FechaDentroMesCerrado_ReturnsTrue()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        var cierre = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = 2025,
            Mes = 5,
            FechaCierre = DateTime.Now,
            UsuarioCierre = "admin@test.com",
            SaldoInicialCalculado = 100000m,
            TotalIngresos = 50000m,
            TotalEgresos = 30000m,
            SaldoFinal = 120000m,
            CreatedAt = DateTime.Now,
            CreatedBy = "admin@test.com"
        };
        context.CierresMensuales.Add(cierre);
        await context.SaveChangesAsync();

        // Act - fecha es 15 de mayo 2025
        var fecha = new DateTime(2025, 5, 15);
        var resultado = await service.EsFechaCerradaAsync(fecha);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task CerrarMesAsync_ValidMes_CreatesCierre()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();

        // Act
        var cierre = await service.CerrarMesAsync(2025, 3, "tesorero@test.com", "Cierre regular de marzo");

        // Assert
        Assert.NotNull(cierre);
        Assert.Equal(2025, cierre.Ano);
        Assert.Equal(3, cierre.Mes);
        Assert.Equal("tesorero@test.com", cierre.UsuarioCierre);
        Assert.Equal("Cierre regular de marzo", cierre.Observaciones);

        // Verificar que se guardó en BD
        var guardado = await context.CierresMensuales
            .FirstOrDefaultAsync(c => c.Ano == 2025 && c.Mes == 3);
        Assert.NotNull(guardado);
    }

    [Fact]
    public async Task CerrarMesAsync_MesYaCerrado_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        
        // Cerrar una vez
        await service.CerrarMesAsync(2025, 4, "admin@test.com");

        // Act & Assert - intentar cerrar de nuevo
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CerrarMesAsync(2025, 4, "admin@test.com")
        );
    }

    [Theory]
    [InlineData(0)]  // Mes inválido: 0
    [InlineData(13)] // Mes inválido: 13
    [InlineData(-1)] // Mes inválido: negativo
    public async Task CerrarMesAsync_MesInvalido_ThrowsArgumentException(int mesInvalido)
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.CerrarMesAsync(2025, mesInvalido, "admin@test.com")
        );
    }

    [Fact]
    public async Task ObtenerCierresAsync_MultipleCierres_ReturnsOrderedByAnoMesDesc()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        
        var cierre1 = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = 2025,
            Mes = 3,
            FechaCierre = DateTime.Now.AddDays(-60),
            UsuarioCierre = "admin@test.com",
            SaldoInicialCalculado = 100000m,
            TotalIngresos = 50000m,
            TotalEgresos = 30000m,
            SaldoFinal = 120000m,
            CreatedAt = DateTime.Now.AddDays(-60),
            CreatedBy = "admin@test.com"
        };

        var cierre2 = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = 2025,
            Mes = 5,
            FechaCierre = DateTime.Now.AddDays(-30),
            UsuarioCierre = "admin@test.com",
            SaldoInicialCalculado = 120000m,
            TotalIngresos = 60000m,
            TotalEgresos = 40000m,
            SaldoFinal = 140000m,
            CreatedAt = DateTime.Now.AddDays(-30),
            CreatedBy = "admin@test.com"
        };

        context.CierresMensuales.AddRange(cierre1, cierre2);
        await context.SaveChangesAsync();

        // Act
        var cierres = await service.ObtenerCierresAsync();

        // Assert
        Assert.NotEmpty(cierres);
        Assert.Equal(2, cierres.Count);
        // Verificar orden descendente: mayo antes que marzo
        Assert.Equal(5, cierres[0].Mes);
        Assert.Equal(3, cierres[1].Mes);
    }

    [Fact]
    public async Task ObtenerUltimoCierreAsync_WithCierres_ReturnsLatest()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        
        await service.CerrarMesAsync(2025, 1, "admin@test.com");
        await service.CerrarMesAsync(2025, 3, "admin@test.com");
        await service.CerrarMesAsync(2025, 12, "admin@test.com");

        // Act
        var ultimoCierre = await service.ObtenerUltimoCierreAsync();

        // Assert
        Assert.NotNull(ultimoCierre);
        Assert.Equal(12, ultimoCierre.Mes);
        Assert.Equal(2025, ultimoCierre.Ano);
    }

    [Fact]
    public async Task ObtenerUltimoCierreAsync_NoCierres_ReturnsNull()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        // No agregar cierres

        // Act
        var ultimoCierre = await service.ObtenerUltimoCierreAsync();

        // Assert
        Assert.Null(ultimoCierre);
    }

    [Fact]
    public async Task CerrarMesAsync_CalculatesSaldoCorrectly()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();

        // Act
        var cierre = await service.CerrarMesAsync(2025, 2, "admin@test.com", "Test saldo");

        // Assert
        // Verificar que los cálculos son consistentes
        var saldoCalculado = cierre.SaldoInicialCalculado + cierre.TotalIngresos - cierre.TotalEgresos;
        Assert.Equal(cierre.SaldoFinal, saldoCalculado);
    }

    [Fact]
    public async Task CerrarMesAsync_RecordsAuditInfo()
    {
        // Arrange
        var (context, service) = await CreateTestContextAsync();
        var usuarioActual = "junta@lama.org.co";

        // Act
        var cierre = await service.CerrarMesAsync(2025, 7, usuarioActual, "Observación test");

        // Assert
        Assert.Equal(usuarioActual, cierre.UsuarioCierre);
        Assert.Equal(usuarioActual, cierre.CreatedBy);
        Assert.NotEqual(default, cierre.FechaCierre);
        Assert.NotEqual(default, cierre.CreatedAt);
    }

    /// <summary>
    /// Mock de IAuditService para aislar tests
    /// </summary>
    private class MockAuditService : IAuditService
    {
        public List<(string Entity, string Action, string User, DateTime Time)> Logs { get; } = new();

        public async Task LogAsync(string entityType, string entityId, string action, string userName, object? newValues = null, object? oldValues = null, string? additionalInfo = null)
        {
            Logs.Add((entityType, action, userName, DateTime.Now));
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Factory de contexto para tests
    /// </summary>
    private class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly AppDbContext _context;

        public TestDbContextFactory(AppDbContext context)
        {
            _context = context;
        }

        public AppDbContext CreateDbContext()
        {
            return _context;
        }

        public async ValueTask<AppDbContext> CreateDbContextAsync()
        {
            return await Task.FromResult(_context);
        }

        public void Dispose() { }
    }
}

