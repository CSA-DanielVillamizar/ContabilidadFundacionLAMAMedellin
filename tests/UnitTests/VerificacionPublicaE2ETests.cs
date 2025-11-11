using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests;

public class VerificacionPublicaE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            // Ensure Razor content root for fallback page
            var dir = Directory.GetCurrentDirectory();
            string? serverProjectPath = null;
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "src", "Server");
                if (Directory.Exists(candidate)) { serverProjectPath = candidate; break; }
                var parent = Directory.GetParent(dir); dir = parent?.FullName;
            }
            if (serverProjectPath != null) builder.UseContentRoot(serverProjectPath);

            builder.ConfigureServices(services =>
            {
                // Replace AppDbContext with SQLite in-memory
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<Server.Data.AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                connection.Open();
                connection.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
                services.AddDbContext<Server.Data.AppDbContext>(options => options.UseSqlite(connection));

                // Build provider and create schema
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task GET_recibo_verificacion_404_para_id_inexistente()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();
        var resp = await client.GetAsync($"/recibo/{Guid.NewGuid()}/verificacion");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GET_certificado_verificacion_404_para_id_inexistente()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();
        var resp = await client.GetAsync($"/certificado/{Guid.NewGuid()}/verificacion");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GET_certificado_verificacion_200_y_html_con_datos()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Seed certificado emitido
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
            var cert = new Server.Models.CertificadoDonacion
            {
                Id = Guid.NewGuid(),
                Ano = DateTime.UtcNow.Year,
                Consecutivo = 1,
                FechaEmision = DateTime.UtcNow,
                FechaDonacion = DateTime.UtcNow.Date,
                TipoIdentificacionDonante = "CC",
                IdentificacionDonante = "1234567890",
                NombreDonante = "María",
                DescripcionDonacion = "Aporte",
                ValorDonacionCOP = 100000,
                FormaDonacion = "Efectivo",
                Estado = Server.Models.EstadoCertificado.Emitido
            };
            db.CertificadosDonacion.Add(cert);
            db.SaveChanges();

            var resp = await client.GetAsync($"/certificado/{cert.Id}/verificacion");
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }
            Assert.Equal("text/html", resp.Content.Headers.ContentType?.MediaType);
            var html = await resp.Content.ReadAsStringAsync();
            Assert.Contains("Certificado CD-", html);
            Assert.Contains("Estado:", html);
        }
    }

    [Fact]
    public async Task GET_recibo_verificacion_200_y_html()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();
        Guid id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
            var rec = new Server.Models.Recibo
            {
                Id = Guid.NewGuid(),
                Serie = "SI",
                Ano = 2025,
                Consecutivo = 10,
                FechaEmision = DateTime.UtcNow,
                TotalCop = 50000,
                Estado = Server.Models.EstadoRecibo.Emitido
            };
            db.Recibos.Add(rec);
            db.SaveChanges();
            id = rec.Id;
        }

        var resp = await client.GetAsync($"/recibo/{id}/verificacion");
        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }
        Assert.Equal("text/html", resp.Content.Headers.ContentType?.MediaType);
        var html2 = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Recibo SI-2025-", html2);
    }
}
