using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Scripts;

/// <summary>
/// Script para actualizar el estado de pagos de mensualidades - Octubre 2025
/// Este script crea recibos retroactivos para reflejar los pagos realizados
/// </summary>
public class ActualizarDeudoresOctubre2025
{
    public static async Task EjecutarAsync(AppDbContext db)
    {
        Console.WriteLine("=== Actualizando estado de deudores - Octubre 2025 ===");
        
        var mensualidad = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD");
        if (mensualidad == null)
        {
            Console.WriteLine("❌ Error: Concepto MENSUALIDAD no encontrado");
            return;
        }

        // Fecha actual de referencia: Octubre 2025
        var fechaActual = new DateOnly(2025, 10, 27);
        
        // ==================================================
        // 1. ACTUALIZAR FECHA DE INGRESO NUEVOS MIEMBROS
        // ==================================================
        Console.WriteLine("\n1️⃣ Actualizando fecha de ingreso nuevos miembros (octubre 2025)...");
        
        var nuevosMiembros = new[]
        {
            "LAURA VIVIAN ASALAZAR MORENO",
            "JOSE JULIAN VILLAMIZAR ARAQUE",
            "GUSTAVO ADOLFO GÓMEZ ZULUAGA",
            "Nelson Augusto Montoya Mataute"
        };

        foreach (var nombreCompleto in nuevosMiembros)
        {
            var miembro = await db.Miembros.FirstOrDefaultAsync(m => 
                m.NombreCompleto.ToUpper() == nombreCompleto.ToUpper());
            
            if (miembro != null)
            {
                miembro.FechaIngreso = new DateOnly(2025, 10, 1);
                miembro.UpdatedAt = DateTime.UtcNow;
                miembro.UpdatedBy = "script_actualizacion_octubre_2025";
                Console.WriteLine($"  ✓ {nombreCompleto}: FechaIngreso = octubre 2025");
            }
            else
            {
                Console.WriteLine($"  ⚠️ {nombreCompleto}: NO ENCONTRADO en la base de datos");
            }
        }

        // ==================================================
        // 2. CREAR RECIBOS PARA MIEMBROS AL DÍA
        // ==================================================
        Console.WriteLine("\n2️⃣ Creando recibos para miembros al día...");
        
        await CrearReciboMensualidad(db, mensualidad, "RAMÓN ANTONIO GONZALEZ CASTAÑO", 
            new DateOnly(2025, 10, 1), 10, "Pago enero-octubre 2025");
        
        await CrearReciboMensualidad(db, mensualidad, "CARLOS ALBERTO ARAQUE BETANCUR", 
            new DateOnly(2025, 12, 1), 12, "Pago enero-diciembre 2025");

        // ==================================================
        // 3. CREAR RECIBOS PARA MIEMBROS CON DEUDA MODERADA
        // ==================================================
        Console.WriteLine("\n3️⃣ Creando recibos para miembros con deuda moderada...");
        
        await CrearReciboMensualidad(db, mensualidad, "MILTON DARIO GOMEZ RIVERA", 
            new DateOnly(2025, 6, 1), 6, "Pago enero-junio 2025");
        
        await CrearReciboMensualidad(db, mensualidad, "DANIEL ANDREY VILLAMIZAR ARAQUE", 
            new DateOnly(2025, 6, 1), 6, "Pago enero-junio 2025");
        
        await CrearReciboMensualidad(db, mensualidad, "ANGELA MARIA RODRIGUEZ", 
            new DateOnly(2025, 9, 1), 9, "Pago enero-septiembre 2025");
        
        await CrearReciboMensualidad(db, mensualidad, "CESAR LEONEL RODRIGUEZ GALAN", 
            new DateOnly(2025, 9, 1), 9, "Pago enero-septiembre 2025");

        // ==================================================
        // 4. CREAR RECIBOS PARA MIEMBROS CON DEUDA ALTA
        // ==================================================
        Console.WriteLine("\n4️⃣ Creando recibos para miembros con deuda alta...");
        
        await CrearReciboMensualidad(db, mensualidad, "GIRLESA MARÍA BUITRAGO", 
            new DateOnly(2025, 1, 1), 1, "Pago enero 2025");

        // ==================================================
        // 5. GUARDAR CAMBIOS
        // ==================================================
        Console.WriteLine("\n💾 Guardando cambios en la base de datos...");
        await db.SaveChangesAsync();
        
        Console.WriteLine("\n✅ Actualización completada exitosamente!");
        Console.WriteLine("\nResumen de deudas esperadas:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("  • NUEVOS MIEMBROS (octubre 2025): 0 meses de deuda");
        Console.WriteLine("  • AL DÍA:");
        Console.WriteLine("      - RAMÓN ANTONIO GONZALEZ CASTAÑO: 0 meses");
        Console.WriteLine("      - CARLOS ALBERTO ARAQUE BETANCUR: 0 meses (adelantado hasta diciembre)");
        Console.WriteLine("  • DEUDA MODERADA:");
        Console.WriteLine("      - MILTON DARIO GOMEZ RIVERA: 4 meses (jul-oct)");
        Console.WriteLine("      - DANIEL ANDREY VILLAMIZAR ARAQUE: 4 meses (jul-oct)");
        Console.WriteLine("      - ANGELA MARIA RODRIGUEZ: 1 mes (octubre)");
        Console.WriteLine("      - CESAR LEONEL RODRIGUEZ GALAN: 1 mes (octubre)");
        Console.WriteLine("  • DEUDA ALTA:");
        Console.WriteLine("      - GIRLESA MARÍA BUITRAGO: 9 meses (feb-oct)");
        Console.WriteLine("  • DEUDA TOTAL (10 meses):");
        Console.WriteLine("      - 16 miembros sin pagos registrados este año");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private static async Task CrearReciboMensualidad(
        AppDbContext db, 
        Concepto mensualidad, 
        string nombreCompleto, 
        DateOnly fechaEmision, 
        int cantidadMeses, 
        string observaciones)
    {
        var miembro = await db.Miembros.FirstOrDefaultAsync(m => 
            m.NombreCompleto.ToUpper() == nombreCompleto.ToUpper());
        
        if (miembro == null)
        {
            Console.WriteLine($"  ⚠️ {nombreCompleto}: NO ENCONTRADO");
            return;
        }

        // Verificar si ya existe un recibo para este miembro en este período
        var reciboExistente = await db.Recibos
            .Include(r => r.Items)
            .Where(r => r.MiembroId == miembro.Id 
                     && r.FechaEmision.Month == fechaEmision.Month 
                     && r.FechaEmision.Year == fechaEmision.Year
                     && r.Items.Any(i => i.ConceptoId == mensualidad.Id))
            .FirstOrDefaultAsync();

        if (reciboExistente != null)
        {
            Console.WriteLine($"  ℹ️ {nombreCompleto}: Ya tiene recibo registrado para {fechaEmision:MMM yyyy}");
            return;
        }

        // Crear nuevo recibo
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            MiembroId = miembro.Id,
            FechaEmision = fechaEmision,
            Estado = EstadoRecibo.Emitido,
            Observaciones = observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "script_actualizacion_octubre_2025",
            Items = new List<ReciboItem>
            {
                new ReciboItem
                {
                    Id = Guid.NewGuid(),
                    ConceptoId = mensualidad.Id,
                    Cantidad = cantidadMeses,
                    ValorUnitarioCOP = mensualidad.ValorCOP,
                    TotalCOP = mensualidad.ValorCOP * cantidadMeses,
                    Observaciones = $"Mensualidades: {cantidadMeses} mes(es)"
                }
            }
        };

        db.Recibos.Add(recibo);
        Console.WriteLine($"  ✓ {nombreCompleto}: Recibo creado - {cantidadMeses} meses hasta {fechaEmision:MMM yyyy}");
    }
}
