using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Server.Models;

namespace Server.Security;

/// <summary>
/// Handler de autorización que valida que el usuario actual tenga 2FA habilitado.
/// - Si el usuario no está autenticado, no autoriza.
/// - Si el usuario está autenticado y su propiedad TwoFactorEnabled es true, autoriza.
/// - Considera período de gracia configurable basado en TwoFactorRequiredSince.
/// </summary>
public sealed class TwoFactorEnabledHandler : AuthorizationHandler<TwoFactorEnabledRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TwoFactorEnforcementOptions _options;

    public TwoFactorEnabledHandler(
        UserManager<ApplicationUser> userManager,
        IOptions<TwoFactorEnforcementOptions> options)
    {
        _userManager = userManager;
        _options = options.Value;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        TwoFactorEnabledRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            // No autenticado -> no autorizar
            return;
        }

        var userId = _userManager.GetUserId(context.User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        // Si el usuario tiene 2FA habilitado, autorizar siempre
        if (user.TwoFactorEnabled)
        {
            context.Succeed(requirement);
            return;
        }

        // Si no tiene 2FA, verificar período de gracia
        if (user.TwoFactorRequiredSince.HasValue && _options.EnforceAfterGracePeriod)
        {
            var gracePeriodEnd = user.TwoFactorRequiredSince.Value.AddDays(_options.GracePeriodDays);
            
            // Si el período de gracia no ha expirado, autorizar temporalmente
            if (DateTime.UtcNow < gracePeriodEnd)
            {
                context.Succeed(requirement);
                return;
            }
            
            // Período de gracia expirado y enforcement activo -> no autorizar
            // El usuario será redirigido a AccessDenied
        }
        else
        {
            // Si enforcement está desactivado o no hay fecha registrada, autorizar
            // (solo mostrar banner de advertencia)
            if (!_options.EnforceAfterGracePeriod)
            {
                context.Succeed(requirement);
            }
        }
    }
}
