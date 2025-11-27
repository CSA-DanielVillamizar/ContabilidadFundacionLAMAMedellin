using Microsoft.AspNetCore.Identity;
using Server.Models;

namespace Server.Data.Seed;

/// <summary>
/// Seed de identidad (roles y usuarios base). Parametrizado para habilitar o no 2FA en cuentas semilla.
/// Soporta creación de usuarios de prueba únicos para tests E2E.
/// </summary>
public static class IdentitySeed
{
    /// <summary>
    /// Ejecuta el seed de identidad.
    /// </summary>
    /// <param name="userManager">UserManager para operaciones de usuario.</param>
    /// <param name="roleManager">RoleManager para creación de roles.</param>
    /// <param name="enableTwoFactorForSeed">Si true, habilita TwoFactorEnabled en usuarios Admin y Tesorero.</param>
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, bool enableTwoFactorForSeed = true)
    {
        // Roles estándar de la aplicación. Nota: se agrega "gerentenegocios" para alinear con denominación real.
        var roles = new[] { "Admin", "Tesorero", "Contador", "Operador", "Auditor", "Junta", "Consulta", "gerentenegocios" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        var hasher = userManager.PasswordHasher;

        // ========================= Usuario Tesorero =========================
        var tesoreroEmail = "tesorero@fundacionlamamedellin.org";
        var existingTesorero = await userManager.FindByEmailAsync(tesoreroEmail);
        if (existingTesorero is null)
        {
            var user = new ApplicationUser { UserName = tesoreroEmail, Email = tesoreroEmail, EmailConfirmed = true, TwoFactorEnabled = enableTwoFactorForSeed, TwoFactorRequiredSince = DateTime.UtcNow };
            var pw = "Tesorero123!"; // Password de pruebas
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Tesorero");
            }
        }
        else
        {
            existingTesorero.PasswordHash = hasher.HashPassword(existingTesorero, "Tesorero123!");
            existingTesorero.EmailConfirmed = true;
            existingTesorero.TwoFactorEnabled = enableTwoFactorForSeed; // Parametrizado para entornos de prueba
            if (existingTesorero.TwoFactorRequiredSince is null)
                existingTesorero.TwoFactorRequiredSince = DateTime.UtcNow; // Inicializar período de gracia si no existe
            await userManager.UpdateAsync(existingTesorero);
            if (!await userManager.IsInRoleAsync(existingTesorero, "Tesorero"))
                await userManager.AddToRoleAsync(existingTesorero, "Tesorero");
        }

        // ========================= Usuario Contador =========================
        var contadorEmail = "contador@fundacionlamamedellin.org";
        var existingContador = await userManager.FindByEmailAsync(contadorEmail);
        if (existingContador is null)
        {
            var user = new ApplicationUser { UserName = contadorEmail, Email = contadorEmail, EmailConfirmed = true }; // 2FA no requerido por defecto
            var pw = "Contador123!"; // Password de pruebas
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Contador");
            }
        }
        else
        {
            existingContador.PasswordHash = hasher.HashPassword(existingContador, "Contador123!");
            existingContador.EmailConfirmed = true;
            await userManager.UpdateAsync(existingContador);
            if (!await userManager.IsInRoleAsync(existingContador, "Contador"))
                await userManager.AddToRoleAsync(existingContador, "Contador");
        }

        // ========================= Usuario Admin =========================
        var adminEmail = "admin@fundacionlamamedellin.org";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, TwoFactorEnabled = enableTwoFactorForSeed, TwoFactorRequiredSince = DateTime.UtcNow };
            var pw = "Admin123!"; // Password de pruebas
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
        else
        {
            existingAdmin.PasswordHash = hasher.HashPassword(existingAdmin, "Admin123!");
            existingAdmin.EmailConfirmed = true;
            existingAdmin.TwoFactorEnabled = enableTwoFactorForSeed; // Parametrizado
            if (existingAdmin.TwoFactorRequiredSince is null)
                existingAdmin.TwoFactorRequiredSince = DateTime.UtcNow;
            await userManager.UpdateAsync(existingAdmin);
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
        }

        // ========================= Usuario Gerencia de Negocios =========================
        var gerenteNegociosEmail = "gerentenegocios@fundacionlamamedellin.org";
        var existingGerenteNegocios = await userManager.FindByEmailAsync(gerenteNegociosEmail);
        if (existingGerenteNegocios is null)
        {
            var user = new ApplicationUser { UserName = gerenteNegociosEmail, Email = gerenteNegociosEmail, EmailConfirmed = true }; // 2FA opcional
            var pw = "Gerenc1aNeg0c10s!2025"; // Cambiar en producción
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "gerentenegocios");
            }
        }
    }

    /// <summary>
    /// Crea o actualiza un usuario de prueba con email único para tests E2E.
    /// Útil para evitar conflictos entre tests paralelos.
    /// </summary>
    /// <param name="userManager">UserManager para operaciones de usuario.</param>
    /// <param name="email">Email único del usuario de prueba (ej: test.admin.GUID@lama.test).</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <param name="role">Rol a asignar.</param>
    /// <param name="enable2FA">Si true, habilita 2FA para este usuario.</param>
    /// <returns>El usuario creado o existente.</returns>
    public static async Task<ApplicationUser> EnsureTestUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        bool enable2FA = false)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TwoFactorEnabled = enable2FA,
                TwoFactorRequiredSince = enable2FA ? DateTime.UtcNow : null
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new InvalidOperationException($"Error creando usuario de prueba {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Actualizar contraseña y estado si ya existe
            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
            user.EmailConfirmed = true;
            user.TwoFactorEnabled = enable2FA;
            user.TwoFactorRequiredSince = enable2FA ? (user.TwoFactorRequiredSince ?? DateTime.UtcNow) : null;
            await userManager.UpdateAsync(user);
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
        return user;
    }

    /// <summary>
    /// Elimina un usuario de prueba por email.
    /// Útil para limpieza después de tests E2E.
    /// </summary>
    public static async Task DeleteTestUserAsync(UserManager<ApplicationUser> userManager, string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
    }
}
