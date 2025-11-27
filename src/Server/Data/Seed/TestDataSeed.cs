using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Data.Seed;

/// <summary>
/// Seed de datos mínimos y controlados para tests E2E.
/// Crea datos predecibles y únicos para evitar conflictos en tests paralelos.
/// </summary>
public static class TestDataSeed
{
    /// <summary>
    /// Crea datos mínimos para tests E2E: usuarios, miembros, conceptos, productos, recibos y certificados.
    /// Usa prefijos únicos (TEST_) para evitar conflictos con datos de producción.
    /// </summary>
    public static async Task SeedAsync(
        AppDbContext db, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Roles de prueba (idempotente)
        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Tesorero");
        await EnsureRoleAsync(roleManager, "Gerente");
        await EnsureRoleAsync(roleManager, "gerentenegocios");
        await EnsureRoleAsync(roleManager, "Junta");
        await EnsureRoleAsync(roleManager, "Consulta");

        // 2. Usuarios de prueba únicos para E2E
        await EnsureTestUserAsync(userManager, "test.admin@lama.test", "Admin", "Test Admin", enable2FA: false);
        await EnsureTestUserAsync(userManager, "test.tesorero@lama.test", "Tesorero", "Test Tesorero", enable2FA: false);
        await EnsureTestUserAsync(userManager, "test.gerente@lama.test", "gerentenegocios", "Test Gerente", enable2FA: false);
        await EnsureTestUserAsync(userManager, "test.consulta@lama.test", "Consulta", "Test Consulta", enable2FA: false);

        // 3. Conceptos de prueba (si no existen)
        await EnsureConceptoAsync(db, "TEST_MENSUALIDAD", "TEST Mensualidad", Moneda.COP, 20000, esIngreso: true, esRecurrente: true, Periodicidad.Mensual);
        await EnsureConceptoAsync(db, "TEST_DONACION", "TEST Donación", Moneda.COP, 0, esIngreso: true, esRecurrente: false, Periodicidad.Unico);
        await EnsureConceptoAsync(db, "TEST_EGRESO", "TEST Egreso", Moneda.COP, 0, esIngreso: false, esRecurrente: false, Periodicidad.Unico);

        // 4. Miembro de prueba
        await EnsureTestMemberAsync(db, "TEST-001", "Juan Pérez Test", "juan.perez@test.com", "3001234567");
        await EnsureTestMemberAsync(db, "TEST-002", "María López Test", "maria.lopez@test.com", "3009876543");

        // 5. Producto de prueba
        await EnsureTestProductAsync(db, "TEST-PROD-001", "Producto Test", TipoProducto.Parche, 25000, 50);

        // 6. Cliente y Proveedor de prueba
        await EnsureTestClienteAsync(db, "TEST-CLI-001", "Cliente Test S.A.S.", "test.cliente@lama.test");
        await EnsureTestProveedorAsync(db, "TEST-PROV-001", "Proveedor Test S.A.S.", "test.proveedor@lama.test");

        // 7. TRM de prueba
        await EnsureTRMAsync(db);

        await db.SaveChangesAsync();
        Console.WriteLine("✓ TestDataSeed: Datos de prueba E2E creados exitosamente");
    }

    /// <summary>
    /// Limpia datos de prueba con prefijo TEST_ para tests E2E.
    /// Útil para ejecutar antes de cada suite de tests.
    /// </summary>
    public static async Task CleanTestDataAsync(AppDbContext db)
    {
        // Eliminar en orden de dependencias (FK constraints)
        await db.Database.ExecuteSqlRawAsync("DELETE FROM DetallesVentasProductos WHERE VentaId IN (SELECT Id FROM VentasProductos WHERE NumeroVenta LIKE 'TEST-%')");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM VentasProductos WHERE NumeroVenta LIKE 'TEST-%'");
        
        await db.Database.ExecuteSqlRawAsync("DELETE FROM DetallesComprasProductos WHERE CompraId IN (SELECT Id FROM ComprasProductos WHERE NumeroCompra LIKE 'TEST-%')");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ComprasProductos WHERE NumeroCompra LIKE 'TEST-%'");
        
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ReciboItems WHERE ReciboId IN (SELECT Id FROM Recibos WHERE Serie = 'TEST')");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Recibos WHERE Serie = 'TEST'");
        
        await db.Database.ExecuteSqlRawAsync("DELETE FROM CertificadosDonacion WHERE NombreDonante LIKE '%Test%'");
        
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Egresos WHERE Proveedor LIKE '%Test%'");
        
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Productos WHERE Codigo LIKE 'TEST-%'");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Clientes WHERE Identificacion LIKE 'TEST-%'");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Proveedores WHERE Nit LIKE 'TEST-%'");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Miembros WHERE Cedula LIKE 'TEST-%'");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Conceptos WHERE Codigo LIKE 'TEST_%'");

        Console.WriteLine("✓ TestDataSeed: Datos de prueba E2E limpiados");
    }

    // ===== Métodos auxiliares privados =====

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task EnsureTestUserAsync(
        UserManager<ApplicationUser> userManager, 
        string email, 
        string role, 
        string displayName,
        bool enable2FA)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TwoFactorEnabled = enable2FA
            };
            var result = await userManager.CreateAsync(user, "Test1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"  ✓ Usuario de prueba creado: {email} ({role})");
            }
        }
    }

    private static async Task EnsureConceptoAsync(
        AppDbContext db, 
        string codigo, 
        string nombre, 
        Moneda moneda, 
        decimal precioBase,
        bool esIngreso,
        bool esRecurrente,
        Periodicidad periodicidad)
    {
        if (!await db.Conceptos.AnyAsync(c => c.Codigo == codigo))
        {
            db.Conceptos.Add(new Concepto
            {
                Codigo = codigo,
                Nombre = nombre,
                Moneda = moneda,
                PrecioBase = precioBase,
                EsIngreso = esIngreso,
                EsRecurrente = esRecurrente,
                Periodicidad = periodicidad
            });
        }
    }

    private static async Task EnsureTestMemberAsync(
        AppDbContext db, 
        string cedula, 
        string nombreCompleto, 
        string email,
        string celular)
    {
        if (!await db.Miembros.AnyAsync(m => m.Cedula == cedula))
        {
            db.Miembros.Add(new Miembro
            {
                Cedula = cedula,
                Documento = cedula,
                NombreCompleto = nombreCompleto,
                Nombres = nombreCompleto.Split(' ')[0],
                Apellidos = nombreCompleto.Contains(' ') ? nombreCompleto.Substring(nombreCompleto.IndexOf(' ') + 1) : "",
                Email = email,
                Celular = celular,
                Telefono = celular,
                Direccion = "Calle Test 123",
                Estado = EstadoMiembro.Activo,
                FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                CreatedBy = "TestSeed",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "TestSeed",
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task EnsureTestProductAsync(
        AppDbContext db,
        string codigo,
        string nombre,
        TipoProducto tipo,
        decimal precioVenta,
        int stock)
    {
        if (!await db.Productos.AnyAsync(p => p.Codigo == codigo))
        {
            db.Productos.Add(new Producto
            {
                Codigo = codigo,
                Nombre = nombre,
                Tipo = tipo,
                PrecioVentaCOP = precioVenta,
                StockActual = stock,
                StockMinimo = 10,
                Activo = true,
                CreatedBy = "TestSeed",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task EnsureTestClienteAsync(
        AppDbContext db,
        string identificacion,
        string nombre,
        string email)
    {
        if (!await db.Clientes.AnyAsync(c => c.Identificacion == identificacion))
        {
            db.Clientes.Add(new Cliente
            {
                Identificacion = identificacion,
                Nombre = nombre,
                Tipo = "Empresa",
                Email = email,
                Telefono = "3001234567",
                Direccion = "Calle Test 456",
                Activo = true,
                CreatedBy = "TestSeed",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task EnsureTestProveedorAsync(
        AppDbContext db,
        string nit,
        string nombre,
        string email)
    {
        if (!await db.Proveedores.AnyAsync(p => p.Nit == nit))
        {
            db.Proveedores.Add(new Proveedor
            {
                Nit = nit,
                Nombre = nombre,
                ContactoEmail = email,
                ContactoTelefono = "3009876543",
                Direccion = "Carrera Test 789",
                Activo = true,
                CreatedBy = "TestSeed",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task EnsureTRMAsync(AppDbContext db)
    {
        if (!await db.TasasCambio.AnyAsync())
        {
            db.TasasCambio.Add(new TasaCambio
            {
                Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
                UsdCop = 4000m,
                Fuente = "TestSeed",
                ObtenidaAutomaticamente = false
            });
        }
    }
}
