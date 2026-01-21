using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Server.Data;
using Server.Models;
using Server.Services.Egresos;
using Xunit;
using Server.Services.CierreContable;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnitTests;

public class EgresosServiceTests
{
    internal sealed class FakeAuditService : Server.Services.Audit.IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null)
            => Task.CompletedTask;
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetEntityLogsAsync(string entityType, string entityId)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
        public Task<System.Collections.Generic.List<Server.Models.AuditLog>> GetRecentLogsAsync(int count = 100)
            => Task.FromResult(new System.Collections.Generic.List<Server.Models.AuditLog>());
    }
    internal sealed class TestEnv : IWebHostEnvironment
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

    private static (AppDbContext db, SqliteConnection conn, string webRoot, IWebHostEnvironment env, CierreContableService cierre) CreateInMemoryDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
    conn.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        var temp = Path.Combine(Path.GetTempPath(), "egresos-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(temp);
    var env = new TestEnv { WebRootPath = temp };
    var audit = new FakeAuditService();
    var cierre = new CierreContableService(new TestDbFactory(conn), audit);
    return (db, conn, temp, env, cierre);
    }

    private static IFormFile MakeFormFile(string name, string contentType, string content)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new FormFile(ms, 0, ms.Length, "Soporte", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task CrearAsync_GuardaArchivo_Y_AsignaUrl()
    {
        var (db, conn, root, env, cierre) = CreateInMemoryDb();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierre, audit);
            var eg = new Egreso
            {
                Fecha = DateTime.UtcNow,
                Categoria = "Operativo",
                Proveedor = "Proveedor A",
                Descripcion = "Compra",
                ValorCop = 12345
            };
            var file = MakeFormFile("comprobante.pdf", "application/pdf", "PDFDATA");
            var creado = await svc.CrearAsync(eg, file, "tester", CancellationToken.None);

            Assert.NotNull(creado.SoporteUrl);
            var expectedPath = Path.Combine(root, "data", "egresos", Path.GetFileName(creado.SoporteUrl!));
            Assert.True(File.Exists(expectedPath));
            Assert.True(db.Egresos.Any(e => e.Id == creado.Id));
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualizarAsync_ReemplazaArchivo_Y_BorraAnterior()
    {
        var (db, conn, root, env, cierre) = CreateInMemoryDb();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierre, audit);
            var eg = new Egreso
            {
                Fecha = DateTime.UtcNow,
                Categoria = "Operativo",
                Proveedor = "Proveedor A",
                Descripcion = "Compra",
                ValorCop = 5000
            };
            var f1 = MakeFormFile("a.txt", "text/plain", "A1");
            var creado = await svc.CrearAsync(eg, f1, "tester");
            var oldPath = Path.Combine(root, "data", "egresos", Path.GetFileName(creado.SoporteUrl!));
            Assert.True(File.Exists(oldPath));

            var update = new Egreso
            {
                Fecha = eg.Fecha,
                Categoria = "Operativo",
                Proveedor = "Proveedor B",
                Descripcion = "Compra actualizada",
                ValorCop = 7000
            };
            var f2 = MakeFormFile("b.jpg", "image/jpeg", "B2");
            var actualizado = await svc.ActualizarAsync(creado.Id, update, f2, "tester");
            Assert.NotNull(actualizado);
            var newPath = Path.Combine(root, "data", "egresos", Path.GetFileName(actualizado!.SoporteUrl!));
            Assert.True(File.Exists(newPath));
            Assert.False(File.Exists(oldPath));
            Assert.Equal("Proveedor B", actualizado.Proveedor);
            Assert.Equal(7000, actualizado.ValorCop);
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
    [Fact]
    public async Task EliminarAsync_BorraDeBd_Y_Archivo()
    {
        var (db, conn, root, env, cierre) = CreateInMemoryDb();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierre, audit);
            var eg = new Egreso
            {
                Fecha = DateTime.UtcNow,
                Categoria = "Operativo",
                Proveedor = "Proveedor A",
                Descripcion = "Compra",
                ValorCop = 1000
            };
            var f = MakeFormFile("c.pdf", "application/pdf", "PDF");
            var creado = await svc.CrearAsync(eg, f, "tester");
            var path = Path.Combine(root, "data", "egresos", Path.GetFileName(creado.SoporteUrl!));
            Assert.True(File.Exists(path));

            var ok = await svc.EliminarAsync(creado.Id);
            Assert.True(ok);
            Assert.False(db.Egresos.Any(e => e.Id == creado.Id));
            Assert.False(File.Exists(path));
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CrearAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange
        var (db, conn, root, env, cierreService) = CreateInMemoryDb();
        
        // Cerrar octubre 2025
        var cierre = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = 2025,
            Mes = 10,
            FechaCierre = DateTime.UtcNow,
            UsuarioCierre = "admin@test.com",
            SaldoInicialCalculado = 100000m,
            TotalIngresos = 50000m,
            TotalEgresos = 30000m,
            SaldoFinal = 120000m,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@test.com"
        };
        db.CierresMensuales.Add(cierre);
        await db.SaveChangesAsync();

        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierreService, audit);
            var eg = new Egreso
            {
                Fecha = new DateTime(2025, 10, 15), // Dentro del mes cerrado
                Categoria = "Operativo",
                Proveedor = "Proveedor Test",
                Descripcion = "Intento en mes cerrado",
                ValorCop = 5000
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await svc.CrearAsync(eg, null, "tester")
            );
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualizarAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange
        var (db, conn, root, env, cierreService) = CreateInMemoryDb();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierreService, audit);

            // Crear egreso en mes abierto
            var eg = new Egreso
            {
                Fecha = new DateTime(2025, 9, 15),
                Categoria = "Operativo",
                Proveedor = "Proveedor A",
                Descripcion = "Egreso original",
                ValorCop = 1000
            };
            var creado = await svc.CrearAsync(eg, null, "tester");

            // Cerrar el mes
            var cierre = new CierreMensual
            {
                Id = Guid.NewGuid(),
                Ano = 2025,
                Mes = 9,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin@test.com",
                SaldoInicialCalculado = 100000m,
                TotalIngresos = 50000m,
                TotalEgresos = 30000m,
                SaldoFinal = 120000m,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin@test.com"
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            var update = new Egreso
            {
                Fecha = new DateTime(2025, 9, 16),
                Categoria = "Actualizado",
                Proveedor = "Proveedor B",
                Descripcion = "Intento actualizar en mes cerrado",
                ValorCop = 2000
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await svc.ActualizarAsync(creado.Id, update, null, "tester")
            );
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task EliminarAsync_MesCerrado_ThrowsInvalidOperationException()
    {
        // Arrange
        var (db, conn, root, env, cierreService) = CreateInMemoryDb();
        var audit = new FakeAuditService();
        var factory = new TestDbFactory(conn);
        try
        {
            var svc = new EgresosService(factory, env, cierreService, audit);

            // Crear egreso
            var eg = new Egreso
            {
                Fecha = new DateTime(2025, 11, 20),
                Categoria = "Operativo",
                Proveedor = "Proveedor Test",
                Descripcion = "Egreso a eliminar",
                ValorCop = 3000
            };
            var creado = await svc.CrearAsync(eg, null, "tester");

            // Cerrar el mes
            var cierre = new CierreMensual
            {
                Id = Guid.NewGuid(),
                Ano = 2025,
                Mes = 11,
                FechaCierre = DateTime.UtcNow,
                UsuarioCierre = "admin@test.com",
                SaldoInicialCalculado = 100000m,
                TotalIngresos = 50000m,
                TotalEgresos = 30000m,
                SaldoFinal = 120000m,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin@test.com"
            };
            db.CierresMensuales.Add(cierre);
            await db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await svc.EliminarAsync(creado.Id)
            );
        }
        finally
        {
            conn.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
