using Server.DTOs.Email;

namespace Server.Services.Email;

/// <summary>
/// Servicio para diagnosticar la conectividad SMTP sin enviar correos.
/// </summary>
public interface IEmailDiagnosticsService
{
    /// <summary>
    /// Realiza una prueba de conexión TCP y STARTTLS (si aplica) contra el servidor SMTP configurado.
    /// No realiza AUTH ni envía correos.
    /// </summary>
    Task<EmailProbeResult> ProbeAsync(CancellationToken ct = default);
}
