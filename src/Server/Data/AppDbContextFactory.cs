using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Server.Data;

/// <summary>
/// Factory para crear instancias de AppDbContext en tiempo de diseño (migraciones EF Core).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Usar SQL Server para migraciones (conexión a LamaMedellin)
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
            b => b.MigrationsAssembly("Server"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
