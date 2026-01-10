namespace Server.Configuration;

/// <summary>
/// Opciones de configuración para Rate Limiting.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Límite global de solicitudes por minuto por IP.
    /// </summary>
    public int GlobalRequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Número máximo de intentos de login permitidos en el período de tiempo.
    /// </summary>
    public int LoginMaxAttempts { get; set; } = 5;

    /// <summary>
    /// Período de tiempo en minutos para la limitación de login.
    /// </summary>
    public int LoginLimitWindowMinutes { get; set; } = 15;
}
