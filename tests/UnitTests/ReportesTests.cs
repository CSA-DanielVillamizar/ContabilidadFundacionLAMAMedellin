using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Reportes;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.Threading;

namespace UnitTests;

public class ReportesTests
{
    private sealed class TestEnv : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    internal sealed class TestDbFactory : IDbContextFactory<AppDbContext>
    {
        private readonly SqliteConnection _conn;
        public TestDbFactory(SqliteConnection conn) { _conn = conn; }
        public AppDbContext CreateDbContext()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
            var db = new AppDbContext(opts);
            db.Database.EnsureCreated();
            return db;
        }
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
    [Fact]
    public async Task ExportarReporteMensualPdf_GeneraArchivoNoVacio()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var factory = new TestDbFactory(conn);

        using (var db = factory.CreateDbContext())
        {
            db.Recibos.Add(new Recibo { FechaEmision = DateTime.UtcNow, Estado = EstadoRecibo.Emitido, TotalCop = 10000m });
            db.SaveChanges();
        }

        var env = new TestEnv();
        var service = new ReportesService(factory, env);
        var pdf = await service.GenerarReporteMensualPdfAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 100); // PDF debe tener contenido (minimal en Testing es ~200+ bytes)
        
        conn.Close();
    }

    [Fact]
    public async Task ExportarReporteMensualExcel_GeneraArchivoNoVacio()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var factory = new TestDbFactory(conn);

        using (var db = factory.CreateDbContext())
        {
            db.Recibos.Add(new Recibo { FechaEmision = DateTime.UtcNow, Estado = EstadoRecibo.Emitido, TotalCop = 15000m });
            db.SaveChanges();
        }

        var env = new TestEnv();
        var service = new ReportesService(factory, env);
        var excel = await service.GenerarReporteMensualExcelAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        Assert.NotNull(excel);
        Assert.True(excel.Length > 1000); // Excel debe tener contenido
        
        conn.Close();
    }

    [Fact]
    public async Task GenerarReporteMensual_SinDatos_RegresaCeros()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var factory = new TestDbFactory(conn);

        // Crear el esquema de base de datos
        using (var db = factory.CreateDbContext())
        {
            // Schema ya creado por TestDbFactory
        }

        var env = new TestEnv();
        var service = new ReportesService(factory, env);
        var res = await service.GenerarReporteMensualAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        Assert.Equal(0m, res.SaldoFinal);
        Assert.Equal(0m, res.Ingresos);
        Assert.Equal(0m, res.Egresos);
        
        conn.Close();
    }
    
    [Fact]
    public async Task GenerarReporteMensual_IncluyeEgresosYIngresos()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var factory = new TestDbFactory(conn);

        using (var db = factory.CreateDbContext())
        {
            // Crear un recibo emitido el mes actual por 20000 COP
            var ahora = DateTime.UtcNow;
            var recibo = new Recibo { FechaEmision = ahora, Estado = EstadoRecibo.Emitido, TotalCop = 20000m };
            db.Recibos.Add(recibo);

            // Agregar un concepto e ítem para reflejar los ingresos en ReciboItems
            var concepto = new Concepto { Codigo = "TEST", Nombre = "Test", Moneda = Moneda.COP, PrecioBase = 20000m, EsRecurrente = false, Periodicidad = Periodicidad.Unico, EsIngreso = true };
            db.Conceptos.Add(concepto);
            db.SaveChanges();
            db.ReciboItems.Add(new ReciboItem
            {
                ReciboId = recibo.Id,
                ConceptoId = concepto.Id,
                Cantidad = 1,
                PrecioUnitarioMonedaOrigen = 20000m,
                MonedaOrigen = Moneda.COP,
                SubtotalCop = 20000m
            });

            // Crear un egreso el mismo mes por 5000 COP
            var egreso = new Egreso { Fecha = ahora, ValorCop = 5000m, Categoria = "Materiales", Proveedor = "ProveedorX", Descripcion = "Compra" };
            db.Egresos.Add(egreso);

            db.SaveChanges();
        }

        var env = new TestEnv();
        var service = new ReportesService(factory, env);
        var res = await service.GenerarReporteMensualAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        Assert.Equal(15000m, res.SaldoFinal);
        Assert.Equal(20000m, res.Ingresos);
        Assert.Equal(5000m, res.Egresos);

        conn.Close();
    }
}
