using Microsoft.AspNetCore.Authorization;

namespace Server.Security;

/// <summary>
/// Requisito de autorización que exige que el usuario tenga habilitada la autenticación de dos factores (2FA).
/// </summary>
public sealed class TwoFactorEnabledRequirement : IAuthorizationRequirement
{
    // Sin datos adicionales: el handler consultará el estado 2FA del usuario actual
}
