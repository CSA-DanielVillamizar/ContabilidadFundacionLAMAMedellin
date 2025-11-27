using System;
using System.Threading.Tasks;
using Xunit;
using Server.Services.Audit;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Server.Models;

namespace ContabilidadLAMAMedellin.Tests
{
    public class AuditoriaTests
    {
        [Fact]
        public async Task RegistrarAccion_PersisteEnDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new AppDbContext(options);

            var httpAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var service = new AuditService(db, httpAccessor);

            await service.LogAsync(
                entityType: "Recibo",
                entityId: "123",
                action: "CREAR",
                userName: "testuser"
            );

            var existe = await db.AuditLogs.AnyAsync(a => a.UserName == "testuser" && a.Action == "CREAR" && a.EntityType == "Recibo");
            Assert.True(existe);
        }
    }
}