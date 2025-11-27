using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server;
using Server.Data;
using Server.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Server.Models;

namespace ContabilidadLAMAMedellin.Tests.E2E.Common
{
    /// <summary>
    /// Factory de aplicación para pruebas E2E/Integración.
    /// Automatiza la inicialización de datos de prueba usando <see cref="TestDataSeed"/>.
    /// NOTA: Para pruebas E2E con Playwright, el servidor debe ejecutarse por separado
    /// ya que Playwright requiere una URL HTTP accesible desde el navegador.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private bool _seedInitialized = false;
        private readonly object _seedLock = new object();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            
            // Habilitar Identity en el entorno de pruebas
            builder.UseSetting("EnableIdentityInTesting", "true");
        }

        /// <summary>
        /// Asegura que el seed de prueba se ejecute una sola vez.
        /// Debe llamarse antes de usar la aplicación en tests.
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
}
