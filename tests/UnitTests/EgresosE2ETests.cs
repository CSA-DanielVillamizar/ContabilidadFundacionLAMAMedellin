using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;

namespace UnitTests
{
    public class EgresosE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private WebApplicationFactory<Program> FactoryWithRole(string role)
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                // Set content root to src/Server so Razor/_Host is found
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
                    // Replace AppDbContext with SQLite in-memory
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<Server.Data.AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    connection.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
                    services.AddDbContext<Server.Data.AppDbContext>(options => options.UseSqlite(connection));

                    // Create schema
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });
        }

        [Fact]
        public async Task POST_GET_DELETE_Egresos_Tesorero_FlujoBasico()
        {
            var factory = FactoryWithRole("Tesorero");
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-Role", "Tesorero");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(DateTime.UtcNow.ToString("o")), "Fecha");
            content.Add(new StringContent("Operativo"), "Categoria");
            content.Add(new StringContent("ProveedorX"), "Proveedor");
            content.Add(new StringContent("Compra de insumos"), "Descripcion");
            content.Add(new StringContent("150000"), "ValorCop");

            var createResp = await client.PostAsync("/api/egresos", content);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var creado = await createResp.Content.ReadFromJsonAsync<EgresoDto>();
            Assert.NotNull(creado);
            Assert.True(creado!.Id != Guid.Empty);

            var list = await client.GetFromJsonAsync<EgresoDto[]>("/api/egresos");
            Assert.NotNull(list);
            Assert.Contains(list!, e => e.Id == creado.Id);

            var delResp = await client.DeleteAsync($"/api/egresos/{creado.Id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
        }

        [Fact]
        public async Task POST_Egresos_Consulta_Forbidden()
        {
            var factory = FactoryWithRole("Consulta");
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-Role", "Consulta");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(DateTime.UtcNow.ToString("o")), "Fecha");
            content.Add(new StringContent("Operativo"), "Categoria");
            content.Add(new StringContent("ProveedorX"), "Proveedor");
            content.Add(new StringContent("Compra de insumos"), "Descripcion");
            content.Add(new StringContent("150000"), "ValorCop");

            var resp = await client.PostAsync("/api/egresos", content);
            if (resp.StatusCode != HttpStatusCode.Forbidden)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 403 Forbidden but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }
        }

        public record EgresoDto(Guid Id, DateTime Fecha, string Categoria, string Proveedor, string Descripcion, decimal ValorCop, string? SoporteUrl);
    }
}
