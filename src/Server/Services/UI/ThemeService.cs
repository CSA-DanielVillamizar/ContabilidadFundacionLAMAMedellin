using Microsoft.JSInterop;

namespace Server.Services.UI;

/// <summary>
/// Servicio para gestionar el tema de la aplicación (claro/oscuro)
/// Coordina entre .NET y JavaScript para persistir preferencias en localStorage
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Tema actual ('light' o 'dark')
    /// </summary>
    string CurrentTheme { get; }
    
    /// <summary>
    /// Evento que se dispara cuando cambia el tema
    /// </summary>
    event Action? OnThemeChanged;
    
    /// <summary>
    /// Inicializa el tema desde localStorage o preferencia del sistema
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Alterna entre tema claro y oscuro
    /// </summary>
    Task ToggleThemeAsync();
    
    /// <summary>
    /// Establece el tema específico
    /// </summary>
    /// <param name="theme">'light' o 'dark'</param>
    Task SetThemeAsync(string theme);
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";
    
    public string CurrentTheme => _currentTheme;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Inicializa el tema desde localStorage o preferencia del sistema
    /// Debe llamarse en OnAfterRenderAsync con firstRender = true
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Obtener tema guardado en localStorage o preferencia del sistema
            var theme = await _jsRuntime.InvokeAsync<string>("themeManager.getTheme");
            
            if (!string.IsNullOrEmpty(theme))
            {
                _currentTheme = theme;
                OnThemeChanged?.Invoke();
            }
        }
        catch (JSException)
        {
            // Si falla JS, mantener tema por defecto 'light'
            _currentTheme = "light";
        }
    }

    /// <summary>
    /// Alterna entre tema claro y oscuro
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == "light" ? "dark" : "light";
        await SetThemeAsync(newTheme);
    }

    /// <summary>
    /// Establece el tema específico y lo persiste en localStorage
    /// </summary>
    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark")
        {
            throw new ArgumentException("El tema debe ser 'light' o 'dark'", nameof(theme));
        }

        _currentTheme = theme;
        
        try
        {
            // Guardar en localStorage y aplicar clase en <html>
            await _jsRuntime.InvokeVoidAsync("themeManager.setTheme", theme);
        }
        catch (JSException)
        {
            // Si falla JS, mantener tema en memoria
        }
        
        OnThemeChanged?.Invoke();
    }
}
