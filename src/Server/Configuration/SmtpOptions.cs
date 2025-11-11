namespace Server.Configuration;

/// <summary>
/// Opciones de configuración para SMTP y envío de correos.
/// </summary>
public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Si es true, enviará automáticamente el PDF del certificado al emitirlo.
    /// </summary>
    public bool SendOnCertificateEmission { get; set; } = true;
}
