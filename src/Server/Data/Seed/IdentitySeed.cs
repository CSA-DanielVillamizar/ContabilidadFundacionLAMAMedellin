using Microsoft.AspNetCore.Identity;
using Server.Models;

namespace Server.Data.Seed;

public static class IdentitySeed
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Tesorero", "Operador", "Auditor", "Junta", "Consulta" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        // Usuario Tesorero
        var tesoreroEmail = "tesorero@fundacionlamamedellin.org";
        var existingTesorero = await userManager.FindByEmailAsync(tesoreroEmail);
        if (existingTesorero is null)
        {
            var user = new ApplicationUser { UserName = tesoreroEmail, Email = tesoreroEmail, EmailConfirmed = true };
            var pw = "T3s0r3r0!2025"; // Cambiar en producción
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Tesorero");
            }
        }

        // Usuario Admin
        var adminEmail = "admin@fundacionlamamedellin.org";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var pw = "Adm1nLAMAMedellin*2025"; // Cambiar en producción
            var res = await userManager.CreateAsync(user, pw);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
