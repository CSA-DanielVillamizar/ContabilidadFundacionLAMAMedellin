using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

var builder = DbContextOptionsBuilder<AppDbContext>();
builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ContabilidadLAMAMedellin;Trusted_Connection=true;TrustServerCertificate=true");

using var db = new AppDbContext(builder.Options);

Console.WriteLine("=== Actualización de Deudores - Octubre 2025 ===\n");

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
    Console.WriteLine("1. Actualizando FechaIngreso para nuevos miembros:");
    await ActualizarFechaIngreso("Laura");
    await ActualizarFechaIngreso("José Julián");
    await ActualizarFechaIngreso("Gustavo");
    await ActualizarFechaIngreso("Nelson");

    // 2. CREAR RECIBOS
    Console.WriteLine("\n2. Creando recibos:");
    await CrearRecibo(mensualidad, "Ramón", 2025, 10, 10);
    await CrearRecibo(mensualidad, "Carlos Alberto", 2025, 12, 12);
    await CrearRecibo(mensualidad, "Milton", 2025, 6, 6);
    await CrearRecibo(mensualidad, "Daniel", 2025, 6, 6);
    await CrearRecibo(mensualidad, "Ángela", 2025, 9, 9);
    await CrearRecibo(mensualidad, "César", 2025, 9, 9);
    await CrearRecibo(mensualidad, "Girlesa", 2025, 1, 1);

    await db.SaveChangesAsync();
    Console.WriteLine("\n✅ Actualización completada exitosamente!");
    Console.WriteLine($"\nRecibos creados: {await db.Recibos.CountAsync(r => r.CreatedBy == "actualizacion_oct_2025")}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

async Task ActualizarFechaIngreso(string nombreBuscar)
{
    var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.NombreCompleto.Contains(nombreBuscar));
    if (miembro != null)
    {
        miembro.FechaIngreso = new DateOnly(2025, 10, 1);
        miembro.UpdatedAt = DateTime.UtcNow;
        miembro.UpdatedBy = "actualizacion_oct_2025";
        Console.WriteLine($"  ✓ {miembro.NombreCompleto}");
    }
    else
    {
        Console.WriteLine($"  ⚠️  '{nombreBuscar}': NO ENCONTRADO");
    }
}

async Task CrearRecibo(Concepto mensualidad, string nombreBuscar, int ano, int mes, int cantidad)
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
