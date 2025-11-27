using Microsoft.AspNetCore.Identity;

namespace Server.Models;

/// <summary>
/// Usuario de la aplicación (Identity).
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Fecha y hora en la que se asignó un rol que requiere 2FA (Admin o Tesorero).
    /// Se usa para calcular el período de gracia antes de hacer 2FA obligatorio.
    /// Null si el usuario nunca ha tenido rol Admin/Tesorero.
    /// </summary>
    public DateTime? TwoFactorRequiredSince { get; set; }

}
