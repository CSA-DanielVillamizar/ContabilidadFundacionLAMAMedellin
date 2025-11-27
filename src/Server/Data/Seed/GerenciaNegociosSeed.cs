using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data.Seed;

/// <summary>
/// Seed de datos de ejemplo para módulo de Gerencia de Negocios (Clientes, Proveedores, Compras).
/// Facilita pruebas E2E y demostración del sistema.
/// </summary>
public static class GerenciaNegociosSeed
{
    /// <summary>
    /// Crea un cliente de ejemplo para demostración y testing.
    /// </summary>
    public static async Task SeedClienteEjemploAsync(AppDbContext db)
    {
        // Verificar si ya existe
        if (await db.Clientes.AnyAsync(c => c.Identificacion == "DEMO-CLI-001"))
        {
            return;
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Identificacion = "DEMO-CLI-001",
            Nombre = "Cliente Ejemplo LAMA",
            Tipo = "Persona Natural",  // String, no enum
            Email = "cliente.demo@lamamedellin.org",
            Telefono = "+57 300 123 4567",
            Direccion = "Calle 10 # 20-30, Medellín",
            Activo = true,
            PuntosFidelizacion = 150,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await db.Clientes.AddAsync(cliente);
        await db.SaveChangesAsync();
        Console.WriteLine($"✓ Cliente de ejemplo creado: {cliente.Nombre} ({cliente.Identificacion})");
    }

    /// <summary>
    /// Crea un proveedor y una compra de ejemplo para demostración y testing.
    /// </summary>
    public static async Task SeedCompraEjemploAsync(AppDbContext db)
    {
        // Verificar si ya existe compra de ejemplo
        if (await db.ComprasProductos.AnyAsync(c => c.NumeroCompra == "COMP-DEMO-001"))
        {
            return;
        }

        // Crear proveedor si no existe
        var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Nit == "DEMO-PROV-001");
        if (proveedor == null)
        {
            proveedor = new Proveedor
            {
                Id = Guid.NewGuid(),
                Nit = "DEMO-PROV-001",
                Nombre = "Proveedor Ejemplo S.A.S.",
                ContactoEmail = "ventas@proveedordemo.com",
                ContactoTelefono = "+57 300 999 8888",
                Direccion = "Carrera 50 # 70-80, Medellín",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            await db.Proveedores.AddAsync(proveedor);
            await db.SaveChangesAsync();
            Console.WriteLine($"✓ Proveedor de ejemplo creado: {proveedor.Nombre} ({proveedor.Nit})");
        }

        // Obtener productos existentes para la compra
        var parche = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "PATCH-LAMA-001");
        var camiseta = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "CAM-L-001");

        if (parche == null || camiseta == null)
        {
            Console.WriteLine("⚠️ No se encontraron productos necesarios para compra de ejemplo");
            return;
        }

        var compra = new CompraProducto
        {
            Id = Guid.NewGuid(),
            NumeroCompra = "COMP-DEMO-001",
            FechaCompra = DateTime.UtcNow.AddDays(-15), // Hace 15 días
            ProveedorId = proveedor.Id,
            NumeroFacturaProveedor = "FACT-PROV-2025-001",
            TotalUSD = 200,
            TrmAplicada = 4300,
            TotalCOP = 860000,
            Estado = EstadoCompra.Pagada,
            Observaciones = "Compra de ejemplo para demostración del sistema",
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            CreatedBy = "System"
        };

        var detalles = new List<DetalleCompraProducto>
        {
            new DetalleCompraProducto
            {
                Id = Guid.NewGuid(),
                CompraId = compra.Id,
                ProductoId = parche.Id,
                Cantidad = 50,
                PrecioUnitarioCOP = 12000,
                SubtotalCOP = 600000
            },
            new DetalleCompraProducto
            {
                Id = Guid.NewGuid(),
                CompraId = compra.Id,
                ProductoId = camiseta.Id,
                Cantidad = 20,
                PrecioUnitarioCOP = 13000,
                SubtotalCOP = 260000
            }
        };

        await db.ComprasProductos.AddAsync(compra);
        await db.DetallesComprasProductos.AddRangeAsync(detalles);
        await db.SaveChangesAsync();

        Console.WriteLine($"✓ Compra de ejemplo creada: {compra.NumeroCompra} de {proveedor.Nombre} - Total: ${compra.TotalCOP:N0}");
    }
}
