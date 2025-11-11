using System.Threading;
using System.Threading.Tasks;

namespace Server.Services.Email;

/// <summary>
/// Servicio de envío de correos electrónicos con soporte para adjuntos.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico.
    /// </summary>
    /// <param name="to">Dirección del destinatario.</param>
    /// <param name="subject">Asunto del correo.</param>
    /// <param name="htmlBody">Cuerpo en HTML.</param>
    /// <param name="attachment">Adjunto opcional (nombre, bytes, contentType).</param>
    /// <param name="ct">CancellationToken opcional.</param>
    Task SendAsync(string to, string subject, string htmlBody, (string fileName, byte[] content, string contentType)? attachment = null, CancellationToken ct = default);
}
