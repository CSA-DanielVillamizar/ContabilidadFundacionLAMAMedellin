using Microsoft.Extensions.Options;

namespace Server.Configuration;

/// <summary>
/// Opciones de configuración para backup automático.
/// </summary>
public class BackupOptions
{
    /// <summary>
    /// Indica si el backup automático está habilitado.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Expresión cron para programar el backup (ej: "0 2 * * 0" = Domingos a las 2 AM).
    /// </summary>
    public string CronSchedule { get; set; } = "0 2 * * 0"; // Domingos a las 2 AM por defecto

    /// <summary>
    /// Ruta donde se guardarán los backups.
    /// </summary>
    public string BackupPath { get; set; } = "Backups";

    /// <summary>
    /// Cantidad de días que se conservarán los backups.
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Servidor SQL Server.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Nombre de la base de datos.
    /// </summary>
    public string? Database { get; set; }
}
