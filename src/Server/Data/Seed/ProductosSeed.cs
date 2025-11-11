using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data.Seed;

/// <summary>
/// Seed de productos de ejemplo para demostración de Gerencia de Negocios.
/// </summary>
public static class ProductosSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Verificar si ya existen productos
        if (await db.Productos.AnyAsync())
        {
            return; // Ya hay productos, no volver a crear
        }

        var productos = new List<Producto>
        {
            // Parches oficiales
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "PATCH-LAMA-001",
                Nombre = "Parche Oficial LAMA Medellín",
                Tipo = TipoProducto.Parche,
                PrecioVentaCOP = 25000,
                PrecioVentaUSD = 7,
                StockActual = 50,
                StockMinimo = 10,
                EsParcheOficial = true,
                Descripcion = "Parche oficial del capítulo Medellín de LAMA",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "PATCH-LAMA-INTER",
                Nombre = "Parche LAMA International",
                Tipo = TipoProducto.Parche,
                PrecioVentaCOP = 30000,
                PrecioVentaUSD = 8,
                StockActual = 25,
                StockMinimo = 5,
                EsParcheOficial = true,
                Descripcion = "Parche oficial de LAMA International",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },

            // Camisetas
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "CAM-M-001",
                Nombre = "Camiseta LAMA Medellín",
                Tipo = TipoProducto.Camiseta,
                PrecioVentaCOP = 50000,
                StockActual = 15,
                StockMinimo = 5,
                Talla = "M",
                Descripcion = "Camiseta oficial color negro con logo",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "CAM-L-001",
                Nombre = "Camiseta LAMA Medellín",
                Tipo = TipoProducto.Camiseta,
                PrecioVentaCOP = 50000,
                StockActual = 20,
                StockMinimo = 5,
                Talla = "L",
                Descripcion = "Camiseta oficial color negro con logo",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "CAM-XL-001",
                Nombre = "Camiseta LAMA Medellín",
                Tipo = TipoProducto.Camiseta,
                PrecioVentaCOP = 50000,
                StockActual = 18,
                StockMinimo = 5,
                Talla = "XL",
                Descripcion = "Camiseta oficial color negro con logo",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },

            // Souvenirs
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "SOUV-LLAVERO-001",
                Nombre = "Llavero LAMA Medellín",
                Tipo = TipoProducto.Llavero,
                PrecioVentaCOP = 15000,
                StockActual = 100,
                StockMinimo = 20,
                Descripcion = "Llavero metálico con logo LAMA",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "SOUV-STICKER-001",
                Nombre = "Sticker LAMA Medellín",
                Tipo = TipoProducto.Sticker,
                PrecioVentaCOP = 5000,
                StockActual = 200,
                StockMinimo = 50,
                Descripcion = "Sticker vinilo resistente al agua",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },

            // Gorras
            new Producto
            {
                Id = Guid.NewGuid(),
                Codigo = "GORRA-001",
                Nombre = "Gorra LAMA Medellín",
                Tipo = TipoProducto.Gorra,
                PrecioVentaCOP = 35000,
                StockActual = 30,
                StockMinimo = 10,
                Descripcion = "Gorra bordada color negro",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await db.Productos.AddRangeAsync(productos);
        await db.SaveChangesAsync();

        Console.WriteLine($"✓ Productos seed: {productos.Count} productos creados");
    }

    /// <summary>
    /// Crea una venta de ejemplo a un miembro local para probar cuentas de cobro.
    /// </summary>
    public static async Task SeedVentaEjemploAsync(AppDbContext db)
    {
        // Verificar si ya existe una venta de ejemplo
        if (await db.VentasProductos.AnyAsync(v => v.NumeroVenta == "VENT-DEMO-001"))
        {
            return; // Ya existe
        }

        // Obtener el primer miembro activo
        var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.Estado == EstadoMiembro.Activo);
        if (miembro == null)
        {
            Console.WriteLine("⚠️ No hay miembros activos, no se puede crear venta de ejemplo");
            return;
        }

        // Obtener algunos productos
        var parche = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "PATCH-LAMA-001");
        var camiseta = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "CAM-L-001");
        var llavero = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "SOUV-LLAVERO-001");

        if (parche == null || camiseta == null || llavero == null)
        {
            Console.WriteLine("⚠️ No se encontraron productos necesarios para venta de ejemplo");
            return;
        }

        var venta = new VentaProducto
        {
            Id = Guid.NewGuid(),
            NumeroVenta = "VENT-DEMO-001",
            FechaVenta = DateTime.UtcNow,
            TipoCliente = TipoCliente.MiembroLocal,
            MiembroId = miembro.Id,
            MetodoPago = MetodoPagoVenta.Transferencia,
            Estado = EstadoVenta.Pendiente,
            Observaciones = "Venta de ejemplo para demostración de cuentas de cobro",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        var detalles = new List<DetalleVentaProducto>
        {
            new DetalleVentaProducto
            {
                Id = Guid.NewGuid(),
                VentaId = venta.Id,
                ProductoId = parche.Id,
                Cantidad = 2,
                PrecioUnitarioCOP = parche.PrecioVentaCOP,
                SubtotalCOP = 2 * parche.PrecioVentaCOP
            },
            new DetalleVentaProducto
            {
                Id = Guid.NewGuid(),
                VentaId = venta.Id,
                ProductoId = camiseta.Id,
                Cantidad = 1,
                PrecioUnitarioCOP = camiseta.PrecioVentaCOP,
                SubtotalCOP = camiseta.PrecioVentaCOP
            },
            new DetalleVentaProducto
            {
                Id = Guid.NewGuid(),
                VentaId = venta.Id,
                ProductoId = llavero.Id,
                Cantidad = 3,
                PrecioUnitarioCOP = llavero.PrecioVentaCOP,
                DescuentoCOP = 5000,
                SubtotalCOP = (3 * llavero.PrecioVentaCOP) - 5000
            }
        };

        venta.TotalCOP = detalles.Sum(d => d.SubtotalCOP);

        await db.VentasProductos.AddAsync(venta);
        await db.DetallesVentasProductos.AddRangeAsync(detalles);
        await db.SaveChangesAsync();

        Console.WriteLine($"✓ Venta de ejemplo creada: {venta.NumeroVenta} para {miembro.NombreCompleto} - Total: ${venta.TotalCOP:N0}");
    }
}
