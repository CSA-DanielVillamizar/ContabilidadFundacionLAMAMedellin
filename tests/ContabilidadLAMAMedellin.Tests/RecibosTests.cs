using System;
using System.Threading.Tasks;
using Xunit;
using Server.Services.Recibos;
using Server.DTOs.Recibos;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Server.Services.Exchange;
using Server.Services.CierreContable;
using Server.Services.Audit;
using Server.Models;

namespace ContabilidadLAMAMedellin.Tests
{
    public class RecibosTests
    {
        [Fact]
        public async Task Emitir_GeneraConsecutivo_Y_PDF()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var db = new AppDbContext(options);

            // Seed: un concepto de ingreso para asociar al recibo
            var concepto = new Concepto { Codigo = "MENSUALIDAD", Nombre = "Mensualidad", PrecioBase = 50000m, EsIngreso = true };
            db.Conceptos.Add(concepto);
            await db.SaveChangesAsync();

            // Dependencias
            var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var env = new TestWebHostEnvironment();
            env.WebRootPath = AppContext.BaseDirectory; // carpeta conocida
            var trm = new TestExchangeRateService(4000m);
            var audit = new NoOpAuditService();
            var factory = new TestDbFactory(options);
            var cierre = new CierreContableService(factory, audit);

            var service = new RecibosService(db, trm, env, cfg, cierre, audit);

            var crear = new CreateReciboDto
            {
                Serie = "LM",
                Ano = DateTime.UtcNow.Year,
                FechaEmision = DateTime.UtcNow,
                Items =
                {
                    new CreateReciboItemDto
                    {
                        ConceptoId = concepto.Id,
                        Cantidad = 1,
                        PrecioUnitarioMonedaOrigen = 50000m,
                        MonedaOrigen = Moneda.COP
                    }
                }
            };

            var id = await service.CreateAsync(crear, "test");
            var ok = await service.EmitirAsync(id, "test");

            Assert.True(ok);
            var pdfBytes = await service.GenerarPdfAsync(id);
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
        }
    }
}
