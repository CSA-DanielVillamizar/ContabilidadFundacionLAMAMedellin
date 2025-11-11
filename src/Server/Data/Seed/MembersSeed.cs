using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Globalization;

namespace Server.Data.Seed;

/// <summary>
/// Clase para cargar los miembros iniciales de L.A.M.A. Medellín desde el archivo CSV.
/// </summary>
public static class MembersSeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Solo cargar si no hay miembros en la base de datos
        if (await db.Miembros.AnyAsync())
        {
            return;
        }

        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "miembros_lama_medellin.csv");
        
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"⚠️ Archivo CSV no encontrado en: {csvPath}");
            return;
        }

        var miembros = new List<Miembro>();
        // Leer el archivo con codificación UTF-8 para preservar tildes, ñ y acentos
        var lines = await File.ReadAllLinesAsync(csvPath, System.Text.Encoding.UTF8);

        // Saltar la primera línea (encabezado)
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Parsear CSV correctamente (manejando comillas y comas dentro de campos)
            var parts = ParseCsvLine(line);
            if (parts.Length < 12) continue;

            try
            {
                var miembro = new Miembro
                {
                    Id = Guid.NewGuid(),
                    NombreCompleto = parts[0],
                    Nombres = parts[1],
                    Apellidos = parts[2],
                    Cedula = parts[3],
                    Documento = parts[3], // Compatibilidad
                    Email = parts[4],
                    Celular = parts[5],
                    Telefono = parts[5], // Compatibilidad
                    Direccion = parts[6],
                    NumeroSocio = int.TryParse(parts[7], out var numSocio) ? numSocio : null,
                    MemberNumber = int.TryParse(parts[7], out var memberNum) ? memberNum : null,
                    Cargo = parts[8],
                    Rango = parts[9],
                    Estado = EstadoMiembro.Activo,
                    FechaIngreso = DateOnly.TryParseExact(parts[11], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) 
                        ? fecha 
                        : DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.UtcNow
                };

                miembros.Add(miembro);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error procesando línea {i + 1}: {ex.Message}");
            }
        }

        if (miembros.Any())
        {
            db.Miembros.AddRange(miembros);
            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Se cargaron {miembros.Count} miembros desde el CSV");
        }
    }

    /// <summary>
    /// Parsea una línea de CSV manejando correctamente campos con comillas y comas.
    /// Preserva todos los caracteres especiales (tildes, ñ, acentos).
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        // Agregar el último campo
        fields.Add(currentField.ToString().Trim());

        return fields.ToArray();
    }

    /// <summary>
    /// Copia el logo de L.A.M.A. Medellín a la carpeta wwwroot/images si no existe.
    /// </summary>
    public static void CopyLogo()
    {
        var sourceLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "LogoLAMAMedellin.png");
        var targetLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "LogoLAMAMedellin.png");

        if (File.Exists(sourceLogoPath) && !File.Exists(targetLogoPath))
        {
            var targetDir = Path.GetDirectoryName(targetLogoPath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir!);
            }

            File.Copy(sourceLogoPath, targetLogoPath, overwrite: true);
            Console.WriteLine($"✅ Logo copiado a: {targetLogoPath}");
        }
        else if (!File.Exists(sourceLogoPath))
        {
            Console.WriteLine($"⚠️ Logo no encontrado en: {sourceLogoPath}");
        }
        else
        {
            Console.WriteLine($"ℹ️ Logo ya existe en: {targetLogoPath}");
        }
    }
}
