using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

var builder = new DbContextOptionsBuilder<AppDbContext>();
builder.UseSqlServer(
    "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;",
    options => options.EnableRetryOnFailure()
);

using var db = new AppDbContext(builder.Options);

Console.WriteLine("=== Actualización de Deudores - Octubre 2025 (Corrección fechas ingreso) ===\n");

try
{
    var mensualidad = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD");
    if (mensualidad == null)
    {
        Console.WriteLine("❌ Error: MENSUALIDAD no encontrado");
        return;
    }

    Console.WriteLine($"✓ Concepto MENSUALIDAD: ${mensualidad.PrecioBase}\n");

    // 1. NUEVOS MIEMBROS - Actualizar FechaIngreso
    Console.WriteLine("1. Corrigiendo FechaIngreso para nuevos miembros:");
    await ActualizarFechaIngreso(db, "Laura Viviana Salazar Moreno", new DateOnly(2025, 6, 4));
    await ActualizarFechaIngreso(db, "José Julián Villamizar Araque", new DateOnly(2025, 6, 4));
    await ActualizarFechaIngreso(db, "Gustavo Adolfo Gómez Zuluaga", new DateOnly(2025, 10, 14));
    await ActualizarFechaIngreso(db, "Nelson Augusto Montoya Mataute", new DateOnly(2025, 10, 20));

    // 2. CREAR RECIBOS
    Console.WriteLine("\n2. Creando recibos:");
    await CrearRecibo(db, mensualidad, "Ramón", 2025, 10, 10);
    await CrearRecibo(db, mensualidad, "Carlos Alberto", 2025, 12, 12);
    await CrearRecibo(db, mensualidad, "Milton", 2025, 6, 6);
    await CrearRecibo(db, mensualidad, "Daniel", 2025, 6, 6);
    await CrearRecibo(db, mensualidad, "Ángela", 2025, 9, 9);
    await CrearRecibo(db, mensualidad, "César", 2025, 9, 9);
    await CrearRecibo(db, mensualidad, "Girlesa", 2025, 1, 1);

    await db.SaveChangesAsync();

    Console.WriteLine("\nVerificación rápida de fechas de ingreso:");
    var verif = await db.Miembros
        .Where(m => new[]
        {
            "Laura Viviana Salazar Moreno",
            "José Julián Villamizar Araque",
            "Gustavo Adolfo Gómez Zuluaga",
            "Nelson Augusto Montoya Mataute"
        }.Contains(m.NombreCompleto))
        .Select(m => new { m.NombreCompleto, m.FechaIngreso })
        .ToListAsync();
    foreach (var v in verif)
    {
        Console.WriteLine($"  • {v.NombreCompleto}: {v.FechaIngreso:yyyy-MM-dd}");
    }

    Console.WriteLine("\n✅ Actualización completada exitosamente!");
    Console.WriteLine($"\nRecibos creados: {await db.Recibos.CountAsync(r => r.CreatedBy == "actualizacion_oct_2025")}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

static async Task ActualizarFechaIngreso(AppDbContext db, string nombreCompleto, DateOnly fecha)
{
    var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.NombreCompleto == nombreCompleto);
    if (miembro != null)
    {
        miembro.FechaIngreso = fecha;
        miembro.UpdatedAt = DateTime.UtcNow;
        miembro.UpdatedBy = "correccion_fecha_ingreso_oct_2025";
        Console.WriteLine($"  ✓ {miembro.NombreCompleto} -> {fecha:yyyy-MM-dd}");
    }
    else
    {
        Console.WriteLine($"  ⚠️  '{nombreCompleto}': NO ENCONTRADO");
    }
}

static async Task CrearRecibo(AppDbContext db, Concepto mensualidad, string nombreBuscar, int ano, int mes, int cantidad)
{
    var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.NombreCompleto.Contains(nombreBuscar));
    if (miembro == null)
    {
        Console.WriteLine($"  ⚠️  '{nombreBuscar}': NO ENCONTRADO");
        return;
    }

    var existe = await db.Recibos.AnyAsync(r => r.MiembroId == miembro.Id && r.FechaEmision.Year == ano && r.FechaEmision.Month == mes);
    if (existe)
    {
        Console.WriteLine($"  ℹ️  {miembro.NombreCompleto}: Ya tiene recibo");
        return;
    }

    var recibo = new Recibo
    {
        Id = Guid.NewGuid(),
        MiembroId = miembro.Id,
        FechaEmision = new DateTime(ano, mes, 1),
        Ano = ano,
        Estado = EstadoRecibo.Emitido,
        TotalCop = mensualidad.PrecioBase * cantidad,
        Observaciones = $"Actualización oct 2025 - {cantidad} meses",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "actualizacion_oct_2025",
        Items = new List<ReciboItem>
        {
            new ReciboItem
            {
                ConceptoId = mensualidad.Id,
                Cantidad = cantidad,
                PrecioUnitarioMonedaOrigen = mensualidad.PrecioBase,
                MonedaOrigen = mensualidad.Moneda,
                SubtotalCop = mensualidad.PrecioBase * cantidad
            }
        }
    };

    db.Recibos.Add(recibo);
    Console.WriteLine($"  ✓ {miembro.NombreCompleto}: {cantidad} meses desde {mes}/{ano}");
}
