using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Text.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace UnitTests;

public class DeudoresE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> CreateFactoryWithSeed()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            // Ensure Razor content root
            var dir = Directory.GetCurrentDirectory();
            string serverProjectPath = null;
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "src", "Server");
                if (Directory.Exists(candidate)) { serverProjectPath = candidate; break; }
                var parent = Directory.GetParent(dir); dir = parent?.FullName;
            }
            if (serverProjectPath != null) builder.UseContentRoot(serverProjectPath);

            builder.ConfigureServices(services =>
            {
                // Replace DbContext with SQLite in-memory
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<Server.Data.AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                connection.Open();
                // Registrar collation usada por SQL Server para compatibilidad en SQLite
                connection.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
                services.AddDbContext<Server.Data.AppDbContext>(options => options.UseSqlite(connection));

                // Build provider and seed minimal data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
                db.Database.EnsureCreated();

                // Seed Concepto MENSUALIDAD and one Miembro activo
                if (!db.Conceptos.Any())
                {
                    db.Conceptos.Add(new Server.Models.Concepto
                    {
                        Codigo = "MENSUALIDAD",
                        Nombre = "Mensualidad",
                        PrecioBase = 20000,
                        Moneda = Server.Models.Moneda.COP,
                        EsIngreso = true,
                        EsRecurrente = true,
                        Periodicidad = Server.Models.Periodicidad.Mensual
                    });
                }
                var miembro = new Server.Models.Miembro
                {
                    Id = Guid.NewGuid(),
                    Nombres = "Juan",
                    Apellidos = "Pérez",
                    Documento = "123",
                    Email = "juan@example.com",
                    Telefono = "3000000000",
                    Estado = Server.Models.EstadoMiembro.Activo,
                    // Usar fecha de ingreso de 3 meses atrás para asegurar que tenga deudas pendientes
                    FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3))
                };
                if (!db.Miembros.Any()) db.Miembros.Add(miembro);
                db.SaveChanges();
            });
        });
    }

    [Fact]
    public async Task GET_Deudores_Returns_List_With_Expected_Shape()
    {
        var factory = CreateFactoryWithSeed();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Consulta");

        var resp = await client.GetAsync("/api/deudores");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);
        if (doc.RootElement.GetArrayLength() > 0)
        {
            var first = doc.RootElement[0];
            Assert.True(first.TryGetProperty("miembroId", out _));
            Assert.True(first.TryGetProperty("nombre", out _));
            Assert.True(first.TryGetProperty("ingreso", out _));
            Assert.True(first.TryGetProperty("mesesPendientes", out var mp) && mp.ValueKind == JsonValueKind.Array);
            Assert.True(first.TryGetProperty("totalEstimadoCop", out _));
        }
    }

    [Fact]
    public async Task POST_GenerarRecibo_Tesorero_Succeeds_Consulta_Forbidden()
    {
        var factory = CreateFactoryWithSeed();

        // Consulta cannot generate
        var consulta = factory.CreateClient();
        consulta.DefaultRequestHeaders.Add("X-Test-Role", "Consulta");
        var bad = await consulta.PostAsJsonAsync("/api/deudores/generar-recibo", new { MiembroId = Guid.NewGuid(), CantidadMeses = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, bad.StatusCode);

        // Tesorero can generate: first read a real MiembroId from GET
        var tesorero = factory.CreateClient();
        tesorero.DefaultRequestHeaders.Add("X-Test-Role", "Tesorero");
    var list = await tesorero.GetFromJsonAsync<JsonElement>("/api/deudores");
    var miembroId = list.EnumerateArray().First().GetProperty("miembroId").GetGuid();
        var ok = await tesorero.PostAsJsonAsync("/api/deudores/generar-recibo", new { MiembroId = miembroId, CantidadMeses = 1 });
        if (ok.StatusCode != HttpStatusCode.OK)
        {
            var body = await ok.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 200 OK but got {(int)ok.StatusCode} {ok.StatusCode}. Body: {body}");
        }
        var payload = await ok.Content.ReadFromJsonAsync<ReciboCreated>();
        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload!.Id);

        // Optional: fetch PDF
        var pdf = await tesorero.GetAsync($"/api/recibos/{payload.Id}/pdf");
        if (pdf.StatusCode != HttpStatusCode.OK)
        {
            var body = await pdf.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 200 OK for PDF but got {(int)pdf.StatusCode} {pdf.StatusCode}. Body: {body}");
        }
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType?.MediaType);
    }

    public class ReciboCreated
    {
        public Guid Id { get; set; }
        public string Serie { get; set; } = string.Empty;
        public int Ano { get; set; }
        public int Consecutivo { get; set; }
    }

    [Fact]
    public async Task GET_DeudoresExcel_Returns_Excel_ContentType()
    {
        var factory = CreateFactoryWithSeed();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Consulta");

        var resp = await client.GetAsync("/api/deudores/excel");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resp.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(resp.Content.Headers.ContentDisposition);
        Assert.Equal("attachment", resp.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Contains("deudores.xlsx", resp.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task GET_DeudoresPdf_Returns_Pdf_ContentType()
    {
        var factory = CreateFactoryWithSeed();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Tesorero");

        var resp = await client.GetAsync("/api/deudores/pdf");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("application/pdf", resp.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(resp.Content.Headers.ContentDisposition);
        Assert.Equal("attachment", resp.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Contains("deudores.pdf", resp.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task GET_DeudoresExcel_WithFilters_ReturnsOK()
    {
        var factory = CreateFactoryWithSeed();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Junta");

        var resp = await client.GetAsync("/api/deudores/excel?desde=2024-01&hasta=2024-12");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resp.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GET_DeudoresPdf_WithFilters_ReturnsOK()
    {
        var factory = CreateFactoryWithSeed();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Tesorero");

        var resp = await client.GetAsync("/api/deudores/pdf?desde=2024-06&hasta=2024-12");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("application/pdf", resp.Content.Headers.ContentType?.MediaType);
    }
}
