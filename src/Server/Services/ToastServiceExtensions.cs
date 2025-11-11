namespace Server.Services;

/// <summary>
/// Métodos de extensión para ToastService que simplifican el uso de variantes comunes.
/// </summary>
public static class ToastServiceExtensions
{
    /// <summary>
    /// Muestra un toast de éxito (variante "success")
    /// </summary>
    public static void ShowSuccess(this ToastService service, string message, int? durationMs = null)
    {
        service.Show(message, "success", durationMs);
    }

    /// <summary>
    /// Muestra un toast de error (variante "danger")
    /// </summary>
    public static void ShowError(this ToastService service, string message, int? durationMs = null)
    {
        service.Show(message, "danger", durationMs);
    }

    /// <summary>
    /// Muestra un toast de advertencia (variante "warning")
    /// </summary>
    public static void ShowWarning(this ToastService service, string message, int? durationMs = null)
    {
        service.Show(message, "warning", durationMs);
    }

    /// <summary>
    /// Muestra un toast informativo (variante "info")
    /// </summary>
    public static void ShowInfo(this ToastService service, string message, int? durationMs = null)
    {
        service.Show(message, "info", durationMs);
    }
}
