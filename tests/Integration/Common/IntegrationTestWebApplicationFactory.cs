using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Data.Seed;

namespace ContabilidadLAMAMedellin.Tests.Integration.Common;

/// <summary>
/// Factory para tests de integración que usa TestServer en memoria.
/// A diferencia de E2E, estos tests NO requieren Playwright ni un servidor HTTP real.
/// Usan HttpClient directamente para hacer requests a la aplicación.
/// </summary>
public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static bool _seedInitialized = false;
    private static readonly object _seedLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("EnableIdentityInTesting", "true");

        base.ConfigureWebHost(builder);
    }

    /// <summary>
    /// Asegura que el seed de prueba se ejecute una sola vez.
    /// </summary>
    public async Task EnsureSeedAsync()
    {
        if (_seedInitialized) return;

        lock (_seedLock)
        {
            if (_seedInitialized) return;

            // Crear cliente para forzar inicialización del host
            using var client = CreateClient();

            using var scope = Services.CreateScope();
            var provider = scope.ServiceProvider;
            
            var db = provider.GetRequiredService<AppDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            Task.Run(async () =>
            {
                await TestDataSeed.SeedAsync(db, userManager, roleManager);
            }).GetAwaiter().GetResult();

            _seedInitialized = true;
        }
    }

    /// <summary>
    /// Limpia los datos de prueba creados por el seed.
    /// </summary>
    public void Clean()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Task.Run(async () =>
        {
            await TestDataSeed.CleanTestDataAsync(db);
        }).GetAwaiter().GetResult();
        _seedInitialized = false;
    }
}
