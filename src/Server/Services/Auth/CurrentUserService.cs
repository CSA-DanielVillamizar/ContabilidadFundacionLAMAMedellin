using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Server.Services.Auth;

/// <summary>
/// Interface para el servicio de usuario actual.
/// Provee métodos para obtener información del usuario autenticado.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el nombre de usuario del usuario actual.
    /// </summary>
    string GetUserName();

    /// <summary>
    /// Obtiene el ID del usuario actual.
    /// </summary>
    string? GetUserId();

    /// <summary>
    /// Verifica si el usuario actual está autenticado.
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Verifica si el usuario actual pertenece a un rol específico.
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Obtiene la dirección IP del cliente actual.
    /// </summary>
    string GetIpAddress();
}

/// <summary>
/// Implementación del servicio de usuario actual para aplicaciones Blazor Server.
/// Obtiene información del usuario autenticado desde el contexto de autenticación.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserName()
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return authState.User?.Identity?.Name ?? "system";
    }

    public string? GetUserId()
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return authState.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public bool IsAuthenticated()
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return authState.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        var authState = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return authState.User?.IsInRole(role) ?? false;
    }

    public string GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "Unknown";

        // Intentar obtener IP real detrás de proxy
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

