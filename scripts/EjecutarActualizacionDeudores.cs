using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

// Script para ejecutar la actualización de deudores directamente
var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ContabilidadLAMAMedellin;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var db = new AppDbContext(optionsBuilder.Options);

Console.WriteLine("=== Iniciando actualización de deudores - Octubre 2025 ===\n");

var mensualidad = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD");
if (mensualidad == null)
{
    Console.WriteLine("❌ Error: Concepto MENSUALIDAD no encontrado");
    return;
}

Console.WriteLine($"✓ Concepto MENSUALIDAD encontrado (Precio: {mensualidad.PrecioBase:C})\n");

// 1. NUEVOS MIEMBROS
Console.WriteLine("1️⃣ Actualizando fecha de ingreso nuevos miembros...");
var nuevosMiembros = new[]
{
    "LAURA VIVIAN ASALAZAR MORENO",
    "JOSE JULIAN VILLAMIZAR ARAQUE",
    "GUSTAVO ADOLFO GÓMEZ ZULUAGA",
    "Nelson Augusto Montoya Mataute"
};

foreach (var nombre in nuevosMiembros)
{
    var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.NombreCompleto.ToUpper() == nombre.ToUpper());
    if (miembro != null)
    {
        miembro.FechaIngreso = new DateOnly(2025, 10, 1);
        miembro.UpdatedAt = DateTime.UtcNow;
        miembro.UpdatedBy = "script_actualizacion_octubre_2025";
        Console.WriteLine($"  ✓ {nombre}");
    }
    else
    {
        Console.WriteLine($"  ⚠️ {nombre}: NO ENCONTRADO");
    }
}

// 2. CREAR RECIBOS
Console.WriteLine("\n2️⃣ Creando recibos de pago...");

await CrearRecibo(db, mensualidad, "RAMÓN ANTONIO GONZALEZ CASTAÑO", 2025, 10, 10);
await CrearRecibo(db, mensualidad, "CARLOS ALBERTO ARAQUE BETANCUR", 2025, 12, 12);
await CrearRecibo(db, mensualidad, "MILTON DARIO GOMEZ RIVERA", 2025, 6, 6);
await CrearRecibo(db, mensualidad, "DANIEL ANDREY VILLAMIZAR ARAQUE", 2025, 6, 6);
await CrearRecibo(db, mensualidad, "ANGELA MARIA RODRIGUEZ", 2025, 9, 9);
await CrearRecibo(db, mensualidad, "CESAR LEONEL RODRIGUEZ GALAN", 2025, 9, 9);
await CrearRecibo(db, mensualidad, "GIRLESA MARÍA BUITRAGO", 2025, 1, 1);

// 3. GUARDAR
Console.WriteLine("\n💾 Guardando cambios...");
var cambios = await db.SaveChangesAsync();
Console.WriteLine($"✅ {cambios} registros actualizados");

Console.WriteLine("\n✅ Actualización completada exitosamente!");

static async Task CrearRecibo(AppDbContext db, Concepto mensualidad, string nombreCompleto, int ano, int mes, int cantidadMeses)
{
    var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.NombreCompleto.ToUpper() == nombreCompleto.ToUpper());
    
    if (miembro == null)
    {
        Console.WriteLine($"  ⚠️ {nombreCompleto}: NO ENCONTRADO");
        return;
    }

    var fechaEmision = new DateTime(ano, mes, 1);

    // Verificar si ya existe
    var existe = await db.Recibos
        .Include(r => r.Items)
        .AnyAsync(r => r.MiembroId == miembro.Id 
                    && r.FechaEmision.Month == mes 
                    && r.FechaEmision.Year == ano
                    && r.Items.Any(i => i.ConceptoId == mensualidad.Id));

    if (existe)
    {
        Console.WriteLine($"  ℹ️ {nombreCompleto}: Ya tiene recibo");
        return;
    }

    var recibo = new Recibo
    {
        Id = Guid.NewGuid(),
        MiembroId = miembro.Id,
        FechaEmision = fechaEmision,
        Ano = ano,
        Estado = EstadoRecibo.Emitido,
        Observaciones = $"Actualización octubre 2025 - {cantidadMeses} mes(es)",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "admin_actualizacion_octubre_2025",
        TotalCop = mensualidad.PrecioBase * cantidadMeses,
        Items = new List<ReciboItem>
        {
            new ReciboItem
            {
                ConceptoId = mensualidad.Id,
                Cantidad = cantidadMeses,
                PrecioUnitarioMonedaOrigen = mensualidad.PrecioBase,
                MonedaOrigen = mensualidad.Moneda,
                SubtotalCop = mensualidad.PrecioBase * cantidadMeses,
                Notas = $"{cantidadMeses} mensualidad(es)"
            }
        }
    };

    db.Recibos.Add(recibo);
    Console.WriteLine($"  ✓ {nombreCompleto}: {cantidadMeses} meses");
}
