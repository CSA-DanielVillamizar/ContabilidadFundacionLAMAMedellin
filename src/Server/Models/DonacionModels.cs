namespace Server.Models;

/// <summary>
/// Certificado de donación para RTE (Régimen Tributario Especial).
/// Cumple con Art. 125-2 y Art. 158-1 del Estatuto Tributario colombiano.
/// </summary>
public class CertificadoDonacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Consecutivo anual del certificado (requerido por DIAN).
    /// </summary>
    public int Ano { get; set; } = DateTime.UtcNow.Year;
    public int Consecutivo { get; set; }
    
    /// <summary>
    /// Fecha de emisión del certificado.
    /// </summary>
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha en que se recibió la donación.
    /// </summary>
    public DateTime FechaDonacion { get; set; }
    
    // DATOS DEL DONANTE
    /// <summary>
    /// Tipo de identificación del donante (CC, NIT, CE, etc.).
    /// </summary>
    public string TipoIdentificacionDonante { get; set; } = "CC";
    
    /// <summary>
    /// Número de identificación del donante.
    /// </summary>
    public string IdentificacionDonante { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo o razón social del donante.
    /// </summary>
    public string NombreDonante { get; set; } = string.Empty;
    
    /// <summary>
    /// Dirección del donante (opcional).
    /// </summary>
    public string? DireccionDonante { get; set; }
    
    /// <summary>
    /// Ciudad del donante (opcional).
    /// </summary>
    public string? CiudadDonante { get; set; }
    
    /// <summary>
    /// Teléfono del donante (opcional).
    /// </summary>
    public string? TelefonoDonante { get; set; }
    
    /// <summary>
    /// Email del donante (opcional).
    /// </summary>
    public string? EmailDonante { get; set; }
    
    // DATOS DE LA DONACIÓN
    /// <summary>
    /// Descripción detallada del bien donado (dinero, especie, etc.).
    /// </summary>
    public string DescripcionDonacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor de la donación en pesos colombianos.
    /// </summary>
    public decimal ValorDonacionCOP { get; set; }
    
    /// <summary>
    /// Forma de pago (Transferencia, Efectivo, Cheque, Especie, etc.).
    /// </summary>
    public string FormaDonacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Destinación específica de la donación (programa, proyecto, etc.).
    /// </summary>
    public string? DestinacionDonacion { get; set; }
    
    /// <summary>
    /// Observaciones adicionales.
    /// </summary>
    public string? Observaciones { get; set; }
    
    // RELACIÓN CON RECIBO (OPCIONAL)
    /// <summary>
    /// Recibo asociado si la donación generó un recibo de caja.
    /// </summary>
    public Guid? ReciboId { get; set; }
    public Recibo? Recibo { get; set; }
    
    // INFORMACIÓN DE LA ENTIDAD DONATARIA
    /// <summary>
    /// NIT de la entidad donataria (L.A.M.A. Medellín).
    /// </summary>
    public string NitEntidad { get; set; } = "900.123.456-7"; // Actualizar con NIT real
    
    /// <summary>
    /// Nombre de la entidad donataria.
    /// </summary>
    public string NombreEntidad { get; set; } = "Fundación L.A.M.A. Medellín";
    
    /// <summary>
    /// Indica si la entidad está inscrita en el RTE.
    /// </summary>
    public bool EntidadRTE { get; set; } = true;
    
    /// <summary>
    /// Número de resolución de calificación RTE (DIAN).
    /// </summary>
    public string? ResolucionRTE { get; set; }
    
    /// <summary>
    /// Fecha de la resolución RTE.
    /// </summary>
    public DateTime? FechaResolucionRTE { get; set; }
    
    // FIRMA Y RESPONSABLES
    /// <summary>
    /// Nombre del representante legal que firma.
    /// </summary>
    public string NombreRepresentanteLegal { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificación del representante legal.
    /// </summary>
    public string IdentificacionRepresentante { get; set; } = string.Empty;
    
    /// <summary>
    /// Cargo del representante legal.
    /// </summary>
    public string CargoRepresentante { get; set; } = "Representante Legal";
    
    /// <summary>
    /// Nombre del contador público (si aplica).
    /// </summary>
    public string? NombreContador { get; set; }
    
    /// <summary>
    /// Tarjeta profesional del contador.
    /// </summary>
    public string? TarjetaProfesionalContador { get; set; }
    
    /// <summary>
    /// Nombre del revisor fiscal (si aplica).
    /// </summary>
    public string? NombreRevisorFiscal { get; set; }
    
    /// <summary>
    /// Tarjeta profesional del revisor fiscal.
    /// </summary>
    public string? TarjetaProfesionalRevisorFiscal { get; set; }
    
    // ESTADO Y AUDITORÍA
    /// <summary>
    /// Estado del certificado (Borrador, Emitido, Anulado).
    /// </summary>
    public EstadoCertificado Estado { get; set; } = EstadoCertificado.Borrador;
    
    /// <summary>
    /// Razón de anulación (si aplica).
    /// </summary>
    public string? RazonAnulacion { get; set; }
    
    /// <summary>
    /// Fecha de anulación (si aplica).
    /// </summary>
    public DateTime? FechaAnulacion { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Estado del certificado de donación.
/// </summary>
public enum EstadoCertificado
{
    /// <summary>
    /// Certificado en borrador, no emitido oficialmente.
    /// </summary>
    Borrador = 0,
    
    /// <summary>
    /// Certificado emitido oficialmente con consecutivo asignado.
    /// </summary>
    Emitido = 1,
    
    /// <summary>
    /// Certificado anulado (requiere razón de anulación).
    /// </summary>
    Anulado = 2
}
