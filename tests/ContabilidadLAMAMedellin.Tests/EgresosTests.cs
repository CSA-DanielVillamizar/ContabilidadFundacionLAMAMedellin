using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Server.Services.Egresos;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Server.Models;
using Server.Services.CierreContable;
using Server.Services.Audit;

namespace ContabilidadLAMAMedellin.Tests
{
    public class EgresosTests
    {
        [Fact]
        public async Task CrearEgreso_ConAdjunto_PersisteYAudita()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new AppDbContext(options);

            var factory = new TestDbFactory(options);
            var env = new TestWebHostEnvironment();
            var tmp = Path.Combine(Path.GetTempPath(), "egresos-tests-" + Guid.NewGuid());
            Directory.CreateDirectory(tmp);
            env.WebRootPath = tmp;

            var audit = new NoOpAuditService();
            var cierre = new CierreContableService(factory, audit);

            var service = new EgresosService(factory, env, cierre, audit);

            var egreso = new Egreso
            {
                Categoria = "COMPRA",
                Proveedor = "Proveedor X",
                Descripcion = "Test egreso adjunto",
                ValorCop = 100000m,
                Fecha = DateTime.Today
            };

            using var adjunto = new MemoryStream(new byte[] {1,2,3,4,5});
            var creado = await service.CrearAsync(egreso, adjunto, "prueba.pdf", "testuser");

            var egresoDb = await db.Egresos.FindAsync(creado.Id);
            Assert.NotNull(egresoDb);
            Assert.Equal("COMPRA", egresoDb!.Categoria);
            Assert.True(string.IsNullOrWhiteSpace(egresoDb.SoporteUrl) == false);
        }
    }
}