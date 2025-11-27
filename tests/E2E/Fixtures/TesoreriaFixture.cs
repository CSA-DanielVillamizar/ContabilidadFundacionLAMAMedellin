using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.Data.Seed;
using Server.Models;
using System;
using System.Threading.Tasks;

namespace ContabilidadLAMAMedellin.Tests.E2E.Fixtures;

/// <summary>
/// Fixture para preparar datos de prueba específicos del módulo de Tesorería.
/// Crea recibos, certificados de donación, conceptos y tasas de cambio de prueba.
/// </summary>
public class TesoreriaFixture : IDisposable
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public TesoreriaFixture(
        IDbContextFactory<AppDbContext> dbFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbFactory = dbFactory;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Inicializa los datos de prueba para el módulo de Tesorería.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        // 1. Limpiar datos de prueba previos
        await TestDataSeed.CleanTestDataAsync(db);

        // 2. Crear datos base de prueba (usuarios, conceptos, miembros, TRM)
        await TestDataSeed.SeedAsync(db, _userManager, _roleManager);

        // 3. Crear datos específicos de Tesorería
        await CreateTesoreriaTestDataAsync(db);

        Console.WriteLine("✓ TesoreriaFixture: Datos de prueba inicializados");
    }

    /// <summary>
    /// Crea datos específicos para pruebas de Tesorería: recibos, certificados, etc.
    /// </summary>
    private async Task CreateTesoreriaTestDataAsync(AppDbContext db)
    {
        // Obtener conceptos y miembro de prueba
        var conceptoMensualidad = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "TEST_MENSUALIDAD");
        var conceptoDonacion = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "TEST_DONACION");
        var miembroPrueba = await db.Miembros.FirstOrDefaultAsync(m => m.Cedula == "TEST-001");

        if (conceptoMensualidad == null || conceptoDonacion == null || miembroPrueba == null)
        {
            throw new InvalidOperationException("No se encontraron conceptos o miembros de prueba. Asegúrese de que TestDataSeed se ejecutó correctamente.");
        }

        // Crear recibos de prueba
        var recibos = new[]
        {
            new Recibo
            {
                Serie = "TEST",
                Ano = DateTime.UtcNow.Year,
                Consecutivo = 1,
                FechaEmision = DateTime.UtcNow.AddDays(-5),
                MiembroId = miembroPrueba.Id,
                Estado = EstadoRecibo.Emitido,
                TotalCop = 100000,
                Observaciones = "Recibo de prueba E2E #1",
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoMensualidad.Id,
                        Cantidad = 5,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 20000,
                        SubtotalCop = 100000
                    }
                }
            },
            new Recibo
            {
                Serie = "TEST",
                Ano = DateTime.UtcNow.Year,
                Consecutivo = 2,
                FechaEmision = DateTime.UtcNow.AddDays(-2),
                MiembroId = miembroPrueba.Id,
                Estado = EstadoRecibo.Emitido,
                TotalCop = 50000,
                Observaciones = "Recibo de prueba E2E #2",
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoDonacion.Id,
                        Cantidad = 1,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 50000,
                        SubtotalCop = 50000
                    }
                }
            }
        };

        await db.Recibos.AddRangeAsync(recibos);
        await db.SaveChangesAsync();

        // Crear certificados de donación de prueba
        var certificados = new[]
        {
            new CertificadoDonacion
            {
                FechaDonacion = DateTime.UtcNow.AddDays(-10),
                TipoIdentificacionDonante = "CC",
                IdentificacionDonante = "TEST-DON-001",
                NombreDonante = "Donante Prueba E2E",
                EmailDonante = "donante.test@lama.test",
                DescripcionDonacion = "Donación en efectivo para actividades sociales",
                ValorDonacionCOP = 500000,
                FormaDonacion = "Transferencia bancaria",
                DestinacionDonacion = "Actividades sociales",
                Estado = EstadoCertificado.Borrador,
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new CertificadoDonacion
            {
                FechaDonacion = DateTime.UtcNow.AddDays(-20),
                TipoIdentificacionDonante = "NIT",
                IdentificacionDonante = "TEST-EMP-001",
                NombreDonante = "Empresa Test S.A.S.",
                EmailDonante = "empresa.test@lama.test",
                DescripcionDonacion = "Donación de insumos para evento",
                ValorDonacionCOP = 2000000,
                FormaDonacion = "Especie",
                DestinacionDonacion = "Evento anual",
                Estado = EstadoCertificado.Emitido,
                Ano = DateTime.UtcNow.Year,
                Consecutivo = 1,
                FechaEmision = DateTime.UtcNow.AddDays(-19),
                NombreRepresentanteLegal = "Test Representante",
                IdentificacionRepresentante = "12345678",
                CargoRepresentante = "Representante Legal",
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow.AddDays(-19)
            }
        };

        await db.CertificadosDonacion.AddRangeAsync(certificados);
        await db.SaveChangesAsync();

        Console.WriteLine($"  ✓ Creados {recibos.Length} recibos de prueba");
        Console.WriteLine($"  ✓ Creados {certificados.Length} certificados de prueba");
    }

    /// <summary>
    /// Limpia los datos de prueba después de las pruebas.
    /// </summary>
    public async Task CleanupAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await TestDataSeed.CleanTestDataAsync(db);
        Console.WriteLine("✓ TesoreriaFixture: Datos de prueba limpiados");
    }

    public void Dispose()
    {
        // Cleanup sincrónico no disponible en fixture, usar CleanupAsync explícitamente
    }
}
