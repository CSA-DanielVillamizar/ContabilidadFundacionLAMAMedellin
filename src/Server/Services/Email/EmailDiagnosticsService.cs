using Server.Configuration;
using Server.DTOs.Email;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace Server.Services.Email;

/// <summary>
/// Implementación de diagnóstico SMTP sin envío de correo.
/// - Resuelve DNS
/// - Conecta por TCP
/// - Si EnableSsl está activo y puerto 587, intenta STARTTLS
/// - Si puerto 465, intenta TLS implícito
/// </summary>
public class EmailDiagnosticsService : IEmailDiagnosticsService
{
    private readonly SmtpOptions _options;
    public EmailDiagnosticsService(IOptions<SmtpOptions> options) => _options = options.Value;

    public async Task<EmailProbeResult> ProbeAsync(CancellationToken ct = default)
    {
        var res = new EmailProbeResult
        {
            Host = _options.Host,
            Port = _options.Port,
            EnableSsl = _options.EnableSsl,
            From = _options.From,
            User = _options.User
        };

        if (string.IsNullOrWhiteSpace(_options.Host) || _options.Port <= 0)
        {
            res.Errors.Add("SMTP no configurado (Host/Port)");
            return res;
        }

        try
        {
            _ = await Dns.GetHostAddressesAsync(_options.Host);
            res.DnsResolved = true;
        }
        catch (Exception ex)
        {
            res.Errors.Add($"DNS error: {ex.Message}");
            return res; // sin DNS no seguimos
        }

        using var tcp = new TcpClient();
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            await tcp.ConnectAsync(_options.Host, _options.Port, cts.Token);
            res.TcpConnected = true;
        }
        catch (Exception ex)
        {
            res.Errors.Add($"TCP connect error: {ex.Message}");
            return res;
        }

        using var netStream = tcp.GetStream();
        using var reader = new StreamReader(netStream);
        using var writer = new StreamWriter(netStream) { AutoFlush = true, NewLine = "\r\n" };

        try
        {
            // Greeting
            res.Greeting = await ReadLineWithTimeoutAsync(reader, ct);

            // Para 587 con STARTTLS
            if (_options.EnableSsl && _options.Port == 587)
            {
                await writer.WriteLineAsync($"EHLO lama.local");
                await ReadMultilineAsync(reader, ct);
                await writer.WriteLineAsync("STARTTLS");
                var startTlsResp = await ReadLineWithTimeoutAsync(reader, ct);
                if (!string.IsNullOrWhiteSpace(startTlsResp) && startTlsResp.StartsWith("220"))
                {
                    using var ssl = new SslStream(netStream, leaveInnerStreamOpen: true, ValidateServerCertificate);
                    await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        TargetHost = _options.Host,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    }, ct);
                    res.StartTlsOk = true;
                    res.TlsProtocol = ssl.SslProtocol.ToString();
                    var cert = ssl.RemoteCertificate as X509Certificate2 ?? new X509Certificate2(ssl.RemoteCertificate!);
                    res.CertificateSubject = cert.Subject;
                    res.CertificateIssuer = cert.Issuer;
                    // cerrar educadamente
                    using var sslReader = new StreamReader(ssl);
                    using var sslWriter = new StreamWriter(ssl) { AutoFlush = true, NewLine = "\r\n" };
                    await sslWriter.WriteLineAsync("EHLO lama.local");
                    await ReadMultilineAsync(sslReader, ct);
                    await sslWriter.WriteLineAsync("QUIT");
                }
                else
                {
                    res.Errors.Add($"STARTTLS no aceptado: {startTlsResp}");
                }
            }
            else if (_options.EnableSsl && _options.Port == 465)
            {
                using var ssl = new SslStream(netStream, leaveInnerStreamOpen: true, ValidateServerCertificate);
                await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = _options.Host,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, ct);
                res.StartTlsOk = true;
                res.TlsProtocol = ssl.SslProtocol.ToString();
                var cert = ssl.RemoteCertificate as X509Certificate2 ?? new X509Certificate2(ssl.RemoteCertificate!);
                res.CertificateSubject = cert.Subject;
                res.CertificateIssuer = cert.Issuer;
                using var sslReader = new StreamReader(ssl);
                using var sslWriter = new StreamWriter(ssl) { AutoFlush = true, NewLine = "\r\n" };
                await sslWriter.WriteLineAsync("EHLO lama.local");
                await ReadMultilineAsync(sslReader, ct);
                await sslWriter.WriteLineAsync("QUIT");
            }
            else
            {
                // sin TLS: sólo saludo y QUIT
                await writer.WriteLineAsync("EHLO lama.local");
                await ReadMultilineAsync(reader, ct);
                await writer.WriteLineAsync("QUIT");
            }
        }
        catch (Exception ex)
        {
            res.Errors.Add($"SMTP probe error: {ex.Message}");
        }

        return res;
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        // Aceptar certificado válido por CA (en producción dejemos validación por defecto)
        return sslPolicyErrors == SslPolicyErrors.None;
    }

    private static async Task<string> ReadLineWithTimeoutAsync(StreamReader reader, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        return await reader.ReadLineAsync(cts.Token) ?? string.Empty;
    }

    private static async Task ReadMultilineAsync(StreamReader reader, CancellationToken ct)
    {
        // líneas SMTP multi-línea: prefijo código + '-' continúa; código + ' ' finaliza
        string? line;
        int lines = 0;
        do
        {
            line = await ReadLineWithTimeoutAsync(reader, ct);
            lines++;
            if (string.IsNullOrEmpty(line)) break;
        }
        while (line.Length > 3 && line[3] == '-');
    }
}
