using Microsoft.Extensions.Options;
using Server.Configuration;
using System.Net;
using System.Net.Mail;

namespace Server.Services.Email;

/// <summary>
/// Implementación SMTP para envío de correos electrónicos.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;

    public SmtpEmailService(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, (string fileName, byte[] content, string contentType)? attachment = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.From))
            throw new InvalidOperationException("SMTP no configurado correctamente (Host/From requeridos)");

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            Credentials = string.IsNullOrWhiteSpace(_options.User)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.User, _options.Password),
            EnableSsl = _options.EnableSsl
        };

        using var msg = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(new MailAddress(to));

        if (attachment.HasValue)
        {
            var att = new Attachment(new System.IO.MemoryStream(attachment.Value.content), attachment.Value.fileName, attachment.Value.contentType);
            msg.Attachments.Add(att);
        }

        using var reg = ct.Register(() => client.SendAsyncCancel());
        await client.SendMailAsync(msg);
    }
}
