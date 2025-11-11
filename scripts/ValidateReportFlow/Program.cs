using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Server.Data;
using Server.Models;
using Server.Services.Reportes;

// Simple env stub
class DummyEnv : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "ValidateReportFlow";
    public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
    public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
}

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var anio = 2025;
        var mes = 11; // noviembre
        var ingresoMonto = 120_000m; // prueba
        var egresoMonto = 50_000m;   // prueba

        // Connection string igual a Server/appsettings.Development.json
        var conn = "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;";

        // Configurar DbContextFactory
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => 
            options.UseSqlServer(conn));
        
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        
        var env = new DummyEnv();
        var svc = new ReportesService(factory, env);

        // Reporte antes
        var before = await svc.GenerarReporteMensualAsync(anio, mes);

        Guid reciboId;
        Guid egresoId;
        int conceptoVentaId;
        
        // Usar contexto dedicado para escritura
        await using (var db = await factory.CreateDbContextAsync())
        {
            // Buscar conceptos necesarios
            var conceptoVenta = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "VENTA_CAMISETAS");
            if (conceptoVenta == null)
            {
                Console.Error.WriteLine("ERROR: No existe concepto VENTA_CAMISETAS");
                return 2;
            }
            conceptoVentaId = conceptoVenta.Id;

            // Crear ingreso (recibo) de prueba en noviembre
            reciboId = Guid.NewGuid();
            var recibo = new Recibo
            {
                Id = reciboId,
                Serie = "TEST",
                Ano = anio,
                Consecutivo = 900000 + DateTime.UtcNow.Second,
                FechaEmision = new DateTime(anio, mes, 2),
                Estado = EstadoRecibo.Emitido,
                TotalCop = ingresoMonto,
                Observaciones = "VALIDATION - ingreso demo",
                TerceroLibre = "Validación",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "validation"
            };
            recibo.Items.Add(new ReciboItem
            {
                ConceptoId = conceptoVentaId,
                Cantidad = 1,
                MonedaOrigen = Moneda.COP,
                PrecioUnitarioMonedaOrigen = ingresoMonto,
                SubtotalCop = ingresoMonto,
                Notas = "VALIDATION"
            });
            db.Recibos.Add(recibo);

            // Crear egreso de prueba en noviembre
            egresoId = Guid.NewGuid();
            db.Egresos.Add(new Egreso
            {
                Id = egresoId,
                Fecha = new DateTime(anio, mes, 3),
                Categoria = "VALIDATION",
                Proveedor = "Validación",
                Descripcion = "Egreso demo",
                ValorCop = egresoMonto,
                UsuarioRegistro = "validation",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "validation"
            });

            await db.SaveChangesAsync();
        }

        // Reporte después
        var after = await svc.GenerarReporteMensualAsync(anio, mes);

        // Comprobaciones
        var ingresosOk = after.Ingresos == before.Ingresos + ingresoMonto;
        var egresosOk = after.Egresos == before.Egresos + egresoMonto;
        var finalOk = after.SaldoFinal == before.SaldoFinal + ingresoMonto - egresoMonto;

        Console.WriteLine($"ANTES  => SI: {before.SaldoInicial:C0} ING: {before.Ingresos:C0} EGR: {before.Egresos:C0} SF: {before.SaldoFinal:C0}");
        Console.WriteLine($"DESPUÉS=> SI: {after.SaldoInicial:C0} ING: {after.Ingresos:C0} EGR: {after.Egresos:C0} SF: {after.SaldoFinal:C0}");
        Console.WriteLine($"Δ INGRESOS = +{ingresoMonto:C0} => {(ingresosOk ? "OK" : "FAIL")}");
        Console.WriteLine($"Δ EGRESOS  = +{egresoMonto:C0} => {(egresosOk ? "OK" : "FAIL")}");
        Console.WriteLine($"Δ SALDO FIN= +{(ingresoMonto-egresoMonto):C0} => {(finalOk ? "OK" : "FAIL")}");

        // Limpieza datos
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.ReciboItems.RemoveRange(db.ReciboItems.Where(i => i.ReciboId == reciboId));
            db.Recibos.RemoveRange(db.Recibos.Where(r => r.Id == reciboId));
            db.Egresos.RemoveRange(db.Egresos.Where(e => e.Id == egresoId));
            await db.SaveChangesAsync();
        }

        var pass = ingresosOk && egresosOk && finalOk;
        Console.WriteLine(pass ? "VALIDATION PASS" : "VALIDATION FAIL");
        return pass ? 0 : 1;
    }
}
