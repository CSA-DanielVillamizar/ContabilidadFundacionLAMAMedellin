using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace UnitTests
{
    public class ReportesE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ReportesE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                // Ensure the test host uses the Server project's content root so Razor pages/_Host are available
                // Walk up until we find the repository root that contains src/Server
                var dir = Directory.GetCurrentDirectory();
                string serverProjectPath = null;
                while (dir != null)
                {
                    var candidate = Path.Combine(dir, "src", "Server");
                    if (Directory.Exists(candidate))
                    {
                        serverProjectPath = candidate;
                        break;
                    }
                    var parent = Directory.GetParent(dir);
                    dir = parent?.FullName;
                }

                if (serverProjectPath != null)
                {
                    builder.UseContentRoot(serverProjectPath);
                }

                builder.ConfigureServices(services =>
                {
                    // Add a startup filter that injects a test principal into HttpContext to bypass authorization
                    services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(sp => new TestAuthStartupFilter());

                    // Replace AppDbContext with SQLite in-memory for tests
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<Server.Data.AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add SQLite in-memory
                    var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    connection.CreateCollation("Modern_Spanish_CI_AS", (x, y) => string.Compare(x, y, new System.Globalization.CultureInfo("es-ES"), System.Globalization.CompareOptions.IgnoreCase));
                    services.AddDbContext<Server.Data.AppDbContext>(options => options.UseSqlite(connection));

                    // Build the provider to create the schema
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<Server.Data.AppDbContext>();
                        db.Database.EnsureCreated();
                    }
                });
            });
        }

        // Eliminado: handler obsoleto no utilizado (ISystemClock)

        [Fact]
        public async Task GET_tesoreria_pdf_returns_attachment_and_content_type()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/reportes/tesoreria/pdf?anio=2025&mes=10");
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }
            Assert.Equal("application/pdf", resp.Content.Headers.ContentType?.MediaType);
            Assert.True(resp.Content.Headers.ContentDisposition != null);
            Assert.Equal("attachment", resp.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task GET_tesoreria_excel_returns_attachment_and_content_type()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/reportes/tesoreria/excel?anio=2025&mes=10");
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resp.Content.Headers.ContentType?.MediaType);
            Assert.True(resp.Content.Headers.ContentDisposition != null);
            Assert.Equal("attachment", resp.Content.Headers.ContentDisposition.DispositionType);
        }
    }

    public class TestAuthStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
    {
        public System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
        {
            return app =>
            {
                // Capture exceptions and write them to the response so tests can see server-side details
                app.Use(async (context, _next) =>
                {
                    try
                    {
                        await _next();
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(ex.ToString());
                    }
                });

                app.Use(async (context, _next) =>
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.Role, "Tesorero") };
                    var identity = new ClaimsIdentity(claims, "Test");
                    context.User = new ClaimsPrincipal(identity);
                    await _next();
                });

                // Show developer exception page so tests can see server-side exception details
                app.UseDeveloperExceptionPage();

                next(app);
            };
        }
    }
}
