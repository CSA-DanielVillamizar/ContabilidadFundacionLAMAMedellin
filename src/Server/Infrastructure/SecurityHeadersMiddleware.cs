namespace Server.Infrastructure;

/// <summary>
/// Middleware que añade cabeceras de seguridad a las respuestas HTTP.
/// Mejora la postura de seguridad contra ataques comunes (XSS, clickjacking, MIME sniffing, etc.).
/// Se aplica automáticamente en entornos de producción.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Añadir cabeceras de seguridad a la respuesta
        var headers = context.Response.Headers;

        // HSTS (HTTP Strict-Transport-Security): Fuerza HTTPS durante 1 año
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Prevenir MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevenir clickjacking (no permitir embeber en iframes)
        headers["X-Frame-Options"] = "DENY";

        // Política de referencia: no enviar origen en requests cross-origin
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Política de permisos: deshabilitar acceso a APIs sensibles
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), usb=()";

        // Content Security Policy: baseline restrictiva
        // Permite scripts/estilos inline necesarios para Blazor, pero restringe fuentes externas
        headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self' wss:; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        _logger.LogDebug("Cabeceras de seguridad añadidas a la respuesta");

        await _next(context);
    }
}

/// <summary>
/// Extensión para registrar el middleware de seguridad en la canalización de requests.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
