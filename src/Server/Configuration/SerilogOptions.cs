namespace Server.Configuration;

/// <summary>
/// Opciones de configuración para Serilog.
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// Nivel mínimo de log (Debug, Information, Warning, Error, Fatal).
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Ruta de archivo para guardar logs (ej: "Logs/app_.txt").
    /// </summary>
    public string FilePath { get; set; } = "Logs/app-.txt";

    /// <summary>
    /// Número máximo de archivos de log retenidos.
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// Tamaño máximo de archivo de log en bytes.
    /// </summary>
    public long FileSizeLimitBytes { get; set; } = 104857600; // 100 MB

    /// <summary>
    /// Formato de salida (ej: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}").
    /// </summary>
    public string OutputTemplate { get; set; } = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
}
