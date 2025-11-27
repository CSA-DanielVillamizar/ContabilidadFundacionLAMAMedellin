using System.Net.Http.Headers;

namespace Server.Infrastructure;

/// <summary>
/// DelegatingHandler que propaga las cookies del contexto HTTP actual
/// a las llamadas HttpClient salientes, permitiendo que las páginas Razor
/// autenticadas puedan llamar a los controllers API en el mismo servidor.
/// </summary>
public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            // Copiar cookies del request entrante al request saliente
            var cookieHeader = httpContext.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }

            // Copiar Authorization header si existe (útil para JWT/Bearer tokens)
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
