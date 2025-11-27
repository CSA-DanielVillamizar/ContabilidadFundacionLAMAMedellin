using System;
using System.Threading.Tasks;
using Xunit;
using Server.Services.Exchange;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContabilidadLAMAMedellin.Tests
{
    public class TrmTests
    {
        [Fact]
        public async Task GetUsdCop_NoDuplicaRegistros()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new AppDbContext(options);

            // Seed: registrar una tasa para hoy
            var fecha = DateOnly.FromDateTime(DateTime.Today);
            db.TasasCambio.Add(new Server.Models.TasaCambio { Fecha = fecha, UsdCop = 4000m, EsOficial = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var cfg = new ConfigurationBuilder().Build();
            var service = new ExchangeRateService(db, cfg);

            // Llamar dos veces no debe insertar duplicados (servicio solo lee)
            var r1 = await service.GetUsdCopAsync(fecha);
            var r2 = await service.GetUsdCopAsync(fecha);

            Assert.Equal(4000m, r1);
            Assert.Equal(4000m, r2);
            var count = await db.TasasCambio.CountAsync(tc => tc.Fecha == fecha);
            Assert.Equal(1, count);
        }
    }
}