namespace Server.Security;

/// <summary>
/// Opciones de configuración para la política de 2FA obligatorio.
/// </summary>
public class TwoFactorEnforcementOptions
{
    /// <summary>
    /// Días de gracia antes de hacer 2FA obligatorio para Admin/Tesorero.
    /// Por defecto: 30 días.
    /// </summary>
    public int GracePeriodDays { get; set; } = 30;

    /// <summary>
    /// Si es true, se bloquea el acceso después del período de gracia.
    /// Si es false, solo se muestra el banner de advertencia.
    /// Por defecto: false (solo advertencia).
    /// </summary>
    public bool EnforceAfterGracePeriod { get; set; } = false;
}
