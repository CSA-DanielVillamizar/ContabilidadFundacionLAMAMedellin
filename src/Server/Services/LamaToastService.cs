using MudBlazor;

namespace Server.Services;

/// <summary>
/// Servicio wrapper para Snackbar/Toast de MudBlazor.
/// Proporciona métodos simples para mostrar notificaciones al usuario.
/// </summary>
public class LamaToastService
{
    private readonly ISnackbar _snackbar;

    public LamaToastService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    /// <summary>
    /// Muestra un mensaje de éxito (verde).
    /// </summary>
    public void ShowSuccess(string message, string? title = null)
    {
        var displayMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        _snackbar.Add(displayMessage, Severity.Success, config =>
        {
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    /// <summary>
    /// Alias para ShowSuccess (compatibilidad con 1 o 2 argumentos).
    /// </summary>
    public void Success(string message, string? title = null) => ShowSuccess(message, title);

    /// <summary>
    /// Muestra un mensaje de error (rojo).
    /// </summary>
    public void ShowError(string message, string? title = null)
    {
        var displayMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        _snackbar.Add(displayMessage, Severity.Error, config =>
        {
            config.VisibleStateDuration = 5000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    /// <summary>
    /// Alias para ShowError (compatibilidad con 1 o 2 argumentos).
    /// </summary>
    public void Error(string message, string? title = null) => ShowError(message, title);

    /// <summary>
    /// Muestra un mensaje de advertencia (amarillo).
    /// </summary>
    public void ShowWarning(string message, string? title = null)
    {
        var displayMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        _snackbar.Add(displayMessage, Severity.Warning, config =>
        {
            config.VisibleStateDuration = 4000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    /// <summary>
    /// Alias para ShowWarning (compatibilidad con 1 o 2 argumentos).
    /// </summary>
    public void Warning(string message, string? title = null) => ShowWarning(message, title);

    /// <summary>
    /// Muestra un mensaje informativo (azul).
    /// </summary>
    public void ShowInfo(string message, string? title = null)
    {
        var displayMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        _snackbar.Add(displayMessage, Severity.Info, config =>
        {
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }

    /// <summary>
    /// Alias para ShowInfo (compatibilidad con 1 o 2 argumentos).
    /// </summary>
    public void Info(string message, string? title = null) => ShowInfo(message, title);

    /// <summary>
    /// Muestra un mensaje genérico (gris).
    /// </summary>
    public void Show(string message, string? title = null)
    {
        var displayMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
        _snackbar.Add(displayMessage, Severity.Normal, config =>
        {
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });
    }
}
