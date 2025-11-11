namespace Server.DTOs.Email;

/// <summary>
/// Resultado de la prueba de conexión SMTP sin envío de correo.
/// </summary>
public class EmailProbeResult
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string From { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;

    public bool DnsResolved { get; set; }
    public bool TcpConnected { get; set; }
    public bool StartTlsOk { get; set; }
    public string? Greeting { get; set; }
    public string? TlsProtocol { get; set; }
    public string? CertificateSubject { get; set; }
    public string? CertificateIssuer { get; set; }

    public List<string> Errors { get; set; } = new();
}
