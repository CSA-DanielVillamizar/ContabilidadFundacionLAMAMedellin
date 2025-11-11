using Microsoft.AspNetCore.Identity;
using Server.Models;
using Server.Services.Audit;

namespace Server.Services.Auth;

/// <summary>
/// Servicio para auditar eventos relacionados con autenticación de dos factores (2FA).
/// Intercepta habilitación, deshabilitación y cambios de configuración 2FA.
/// </summary>
public interface ITwoFactorAuditService
{
    /// <summary>
    /// Registra la habilitación de 2FA para un usuario.
    /// </summary>
    Task LogTwoFactorEnabledAsync(ApplicationUser user);

    /// <summary>
    /// Registra la deshabilitación de 2FA para un usuario.
    /// </summary>
    Task LogTwoFactorDisabledAsync(ApplicationUser user);

    /// <summary>
    /// Registra cuando un usuario genera nuevos códigos de recuperación.
    /// </summary>
    Task LogRecoveryCodesGeneratedAsync(ApplicationUser user);

    /// <summary>
    /// Registra cuando un usuario restablece su autenticador.
    /// </summary>
    Task LogAuthenticatorResetAsync(ApplicationUser user);
}

public class TwoFactorAuditService : ITwoFactorAuditService
{
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TwoFactorAuditService(IAuditService auditService, UserManager<ApplicationUser> userManager)
    {
        _auditService = auditService;
        _userManager = userManager;
    }

    public async Task LogTwoFactorEnabledAsync(ApplicationUser user)
    {
        await _auditService.LogAsync(
            entityType: "UserAccount",
            entityId: user.Id,
            action: "2FA_ENABLED",
            userName: user.Email ?? user.UserName ?? "Unknown",
            newValues: new
            {
                TwoFactorEnabled = true,
                Timestamp = DateTime.UtcNow,
                Email = user.Email
            },
            additionalInfo: $"Usuario {user.Email} habilitó autenticación de dos factores (2FA)"
        );
    }

    public async Task LogTwoFactorDisabledAsync(ApplicationUser user)
    {
        await _auditService.LogAsync(
            entityType: "UserAccount",
            entityId: user.Id,
            action: "2FA_DISABLED",
            userName: user.Email ?? user.UserName ?? "Unknown",
            oldValues: new
            {
                TwoFactorEnabled = true
            },
            newValues: new
            {
                TwoFactorEnabled = false,
                Timestamp = DateTime.UtcNow,
                Email = user.Email
            },
            additionalInfo: $"Usuario {user.Email} deshabilitó autenticación de dos factores (2FA)"
        );
    }

    public async Task LogRecoveryCodesGeneratedAsync(ApplicationUser user)
    {
        await _auditService.LogAsync(
            entityType: "UserAccount",
            entityId: user.Id,
            action: "2FA_RECOVERY_CODES_GENERATED",
            userName: user.Email ?? user.UserName ?? "Unknown",
            newValues: new
            {
                Timestamp = DateTime.UtcNow,
                Email = user.Email
            },
            additionalInfo: $"Usuario {user.Email} generó nuevos códigos de recuperación 2FA"
        );
    }

    public async Task LogAuthenticatorResetAsync(ApplicationUser user)
    {
        await _auditService.LogAsync(
            entityType: "UserAccount",
            entityId: user.Id,
            action: "2FA_AUTHENTICATOR_RESET",
            userName: user.Email ?? user.UserName ?? "Unknown",
            newValues: new
            {
                Timestamp = DateTime.UtcNow,
                Email = user.Email
            },
            additionalInfo: $"Usuario {user.Email} restableció su aplicación autenticadora 2FA"
        );
    }
}
