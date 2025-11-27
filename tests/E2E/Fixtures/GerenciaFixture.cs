using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.Data.Seed;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContabilidadLAMAMedellin.Tests.E2E.Fixtures;

/// <summary>
/// Fixture para preparar datos de prueba específicos del módulo de Gerencia de Negocios.
/// Crea productos, clientes, proveedores, compras y ventas de prueba.
/// </summary>
public class GerenciaFixture : IDisposable
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GerenciaFixture(
        IDbContextFactory<AppDbContext> dbFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbFactory = dbFactory;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Inicializa los datos de prueba para el módulo de Gerencia de Negocios.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        // 1. Limpiar datos de prueba previos
        await TestDataSeed.CleanTestDataAsync(db);

        // 2. Crear datos base de prueba (usuarios, conceptos, miembros, TRM)
        await TestDataSeed.SeedAsync(db, _userManager, _roleManager);

        // 3. Crear datos específicos de Gerencia de Negocios
        await CreateGerenciaTestDataAsync(db);

        Console.WriteLine("✓ GerenciaFixture: Datos de prueba inicializados");
    }

    /// <summary>
    /// Crea datos específicos para pruebas de Gerencia: productos, clientes, proveedores, etc.
    /// </summary>
    private async Task CreateGerenciaTestDataAsync(AppDbContext db)
    {
        // Obtener entidades base de prueba
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == "TEST-PROD-001");
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Identificacion == "TEST-CLI-001");
        var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Nit == "TEST-PROV-001");
        var miembro = await db.Miembros.FirstOrDefaultAsync(m => m.Cedula == "TEST-001");

        if (producto == null || cliente == null || proveedor == null)
        {
            throw new InvalidOperationException("No se encontraron productos, clientes o proveedores de prueba. Asegúrese de que TestDataSeed se ejecutó correctamente.");
        }

        // Crear productos adicionales para variedad
        var productosAdicionales = new[]
        {
            new Producto
            {
                Codigo = "TEST-PROD-002",
                Nombre = "Camiseta Test",
                Tipo = TipoProducto.Camiseta,
                PrecioVentaCOP = 60000,
                StockActual = 30,
                StockMinimo = 10,
                Talla = "L",
                Activo = true,
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow
            },
            new Producto
            {
                Codigo = "TEST-PROD-003",
                Nombre = "Gorra Test",
                Tipo = TipoProducto.Gorra,
                PrecioVentaCOP = 35000,
                StockActual = 5, // Bajo stock para probar alertas
                StockMinimo = 10,
                Activo = true,
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow
            }
        };

        await db.Productos.AddRangeAsync(productosAdicionales);
        await db.SaveChangesAsync();

        // Crear compra de prueba
        var compra = new CompraProducto
        {
            NumeroCompra = "TEST-COMP-001",
            FechaCompra = DateTime.UtcNow.AddDays(-7),
            ProveedorId = proveedor.Id,
            NumeroFacturaProveedor = "FACT-TEST-001",
            TotalUSD = 150,
            TrmAplicada = 4000,
            TotalCOP = 600000,
            Estado = EstadoCompra.Pagada,
            Observaciones = "Compra de prueba E2E",
            CreatedBy = "E2E-Test",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        var detallesCompra = new[]
        {
            new DetalleCompraProducto
            {
                CompraId = compra.Id,
                ProductoId = producto.Id,
                Cantidad = 30,
                PrecioUnitarioCOP = 15000,
                SubtotalCOP = 450000
            },
            new DetalleCompraProducto
            {
                CompraId = compra.Id,
                ProductoId = productosAdicionales[0].Id,
                Cantidad = 10,
                PrecioUnitarioCOP = 15000,
                SubtotalCOP = 150000
            }
        };

        await db.ComprasProductos.AddAsync(compra);
        await db.DetallesComprasProductos.AddRangeAsync(detallesCompra);
        await db.SaveChangesAsync();

        // Crear ventas de prueba
        if (miembro != null)
        {
            var venta = new VentaProducto
            {
                NumeroVenta = "TEST-VENT-001",
                FechaVenta = DateTime.UtcNow.AddDays(-3),
                TipoCliente = TipoCliente.MiembroLocal,
                MiembroId = miembro.Id,
                MetodoPago = MetodoPagoVenta.Transferencia,
                Estado = EstadoVenta.Pendiente,
                Observaciones = "Venta de prueba E2E",
                CreatedBy = "E2E-Test",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var detallesVenta = new[]
            {
                new DetalleVentaProducto
                {
                    VentaId = venta.Id,
                    ProductoId = producto.Id,
                    Cantidad = 2,
                    PrecioUnitarioCOP = 25000,
                    SubtotalCOP = 50000
                },
                new DetalleVentaProducto
                {
                    VentaId = venta.Id,
                    ProductoId = productosAdicionales[0].Id,
                    Cantidad = 1,
                    PrecioUnitarioCOP = 60000,
                    SubtotalCOP = 60000
                }
            };

            venta.TotalCOP = detallesVenta.Sum(d => d.SubtotalCOP);

            await db.VentasProductos.AddAsync(venta);
            await db.DetallesVentasProductos.AddRangeAsync(detallesVenta);
            await db.SaveChangesAsync();

            Console.WriteLine($"  ✓ Creada 1 venta de prueba");
        }

        Console.WriteLine($"  ✓ Creados {productosAdicionales.Length + 1} productos de prueba");
        Console.WriteLine($"  ✓ Creada 1 compra de prueba");
    }

    /// <summary>
    /// Limpia los datos de prueba después de las pruebas.
    /// </summary>
    public async Task CleanupAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await TestDataSeed.CleanTestDataAsync(db);
        Console.WriteLine("✓ GerenciaFixture: Datos de prueba limpiados");
    }

    public void Dispose()
    {
        // Cleanup sincrónico no disponible en fixture, usar CleanupAsync explícitamente
    }
}
