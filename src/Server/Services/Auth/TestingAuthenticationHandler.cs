using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Server.Services.Auth;

/// <summary>
/// AuthenticationHandler de pruebas que extrae el rol desde encabezado X-Test-Role o usa 'Tesorero' por defecto.
/// Permite que AuthorizationMiddleware realice Forbid/Challenge sin Identity real.
/// </summary>
public class TestingAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestingAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue("X-Test-Role", out var values) ? values.ToString() : "Tesorero";
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
