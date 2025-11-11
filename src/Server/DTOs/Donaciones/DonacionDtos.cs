using System.ComponentModel.DataAnnotations;
using Server.Models;

namespace Server.DTOs.Donaciones;

/// <summary>
/// DTO para crear un nuevo certificado de donación.
/// </summary>
public class CreateCertificadoDonacionDto
{
    [Required(ErrorMessage = "La fecha de la donación es requerida")]
    public DateTime FechaDonacion { get; set; } = DateTime.Today;

    // DATOS DEL DONANTE
    [Required(ErrorMessage = "El tipo de identificación es requerido")]
    [StringLength(10, ErrorMessage = "El tipo de identificación no puede exceder 10 caracteres")]
    public string TipoIdentificacionDonante { get; set; } = "CC";

    [Required(ErrorMessage = "La identificación del donante es requerida")]
    [StringLength(50, ErrorMessage = "La identificación no puede exceder 50 caracteres")]
    public string IdentificacionDonante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del donante es requerido")]
    [StringLength(300, ErrorMessage = "El nombre no puede exceder 300 caracteres")]
    public string NombreDonante { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DireccionDonante { get; set; }

    [StringLength(150)]
    public string? CiudadDonante { get; set; }

    [StringLength(50)]
    public string? TelefonoDonante { get; set; }

    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(200)]
    public string? EmailDonante { get; set; }

    // DATOS DE LA DONACIÓN
    [Required(ErrorMessage = "La descripción de la donación es requerida")]
    [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
    public string DescripcionDonacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor de la donación es requerido")]
    [Range(1, 999999999999, ErrorMessage = "El valor debe ser mayor a cero")]
    public decimal ValorDonacionCOP { get; set; }

    [Required(ErrorMessage = "La forma de donación es requerida")]
    [StringLength(100)]
    public string FormaDonacion { get; set; } = "Transferencia bancaria";

    [StringLength(1000)]
    public string? DestinacionDonacion { get; set; }

    [StringLength(4000)]
    public string? Observaciones { get; set; }

    // RELACIÓN CON RECIBO
    public Guid? ReciboId { get; set; }
}

/// <summary>
/// DTO para actualizar un certificado de donación (solo en estado Borrador).
/// </summary>
public class UpdateCertificadoDonacionDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "La fecha de la donación es requerida")]
    public DateTime FechaDonacion { get; set; }

    // DATOS DEL DONANTE
    [Required(ErrorMessage = "El tipo de identificación es requerido")]
    [StringLength(10)]
    public string TipoIdentificacionDonante { get; set; } = "CC";

    [Required(ErrorMessage = "La identificación del donante es requerida")]
    [StringLength(50)]
    public string IdentificacionDonante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del donante es requerido")]
    [StringLength(300)]
    public string NombreDonante { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DireccionDonante { get; set; }

    [StringLength(150)]
    public string? CiudadDonante { get; set; }

    [StringLength(50)]
    public string? TelefonoDonante { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? EmailDonante { get; set; }

    // DATOS DE LA DONACIÓN
    [Required(ErrorMessage = "La descripción de la donación es requerida")]
    [StringLength(2000)]
    public string DescripcionDonacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor de la donación es requerido")]
    [Range(1, 999999999999)]
    public decimal ValorDonacionCOP { get; set; }

    [Required(ErrorMessage = "La forma de donación es requerida")]
    [StringLength(100)]
    public string FormaDonacion { get; set; } = "Transferencia bancaria";

    [StringLength(1000)]
    public string? DestinacionDonacion { get; set; }

    [StringLength(4000)]
    public string? Observaciones { get; set; }

    public Guid? ReciboId { get; set; }
}

/// <summary>
/// DTO para lista de certificados (vista general).
/// </summary>
public class CertificadoDonacionListItem
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Consecutivo { get; set; }
    public string NumeroCompleto => $"CD-{Ano}-{Consecutivo:D5}";
    public DateTime FechaEmision { get; set; }
    public DateTime FechaDonacion { get; set; }
    public string NombreDonante { get; set; } = string.Empty;
    public string IdentificacionDonante { get; set; } = string.Empty;
    public decimal ValorDonacionCOP { get; set; }
    public EstadoCertificado Estado { get; set; }
}

/// <summary>
/// DTO con los detalles completos de un certificado.
/// </summary>
public class CertificadoDonacionDetailDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Consecutivo { get; set; }
    public string NumeroCompleto => $"CD-{Ano}-{Consecutivo:D5}";
    
    public DateTime FechaEmision { get; set; }
    public DateTime FechaDonacion { get; set; }
    
    // DONANTE
    public string TipoIdentificacionDonante { get; set; } = string.Empty;
    public string IdentificacionDonante { get; set; } = string.Empty;
    public string NombreDonante { get; set; } = string.Empty;
    public string? DireccionDonante { get; set; }
    public string? CiudadDonante { get; set; }
    public string? TelefonoDonante { get; set; }
    public string? EmailDonante { get; set; }
    
    // DONACIÓN
    public string DescripcionDonacion { get; set; } = string.Empty;
    public decimal ValorDonacionCOP { get; set; }
    public string FormaDonacion { get; set; } = string.Empty;
    public string? DestinacionDonacion { get; set; }
    public string? Observaciones { get; set; }
    
    // RECIBO
    public Guid? ReciboId { get; set; }
    public string? ReciboNumero { get; set; }
    
    // ENTIDAD
    public string NitEntidad { get; set; } = string.Empty;
    public string NombreEntidad { get; set; } = string.Empty;
    public bool EntidadRTE { get; set; }
    public string? ResolucionRTE { get; set; }
    public DateTime? FechaResolucionRTE { get; set; }
    
    // FIRMANTES
    public string NombreRepresentanteLegal { get; set; } = string.Empty;
    public string IdentificacionRepresentante { get; set; } = string.Empty;
    public string CargoRepresentante { get; set; } = string.Empty;
    public string? NombreContador { get; set; }
    public string? TarjetaProfesionalContador { get; set; }
    public string? NombreRevisorFiscal { get; set; }
    public string? TarjetaProfesionalRevisorFiscal { get; set; }
    
    // ESTADO
    public EstadoCertificado Estado { get; set; }
    public string? RazonAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    
    // AUDITORÍA
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO para emitir un certificado (cambiar de Borrador a Emitido).
/// </summary>
public class EmitirCertificadoDto
{
    [Required]
    public Guid Id { get; set; }
    
    // Información de firma (se puede completar al momento de emitir)
    [Required(ErrorMessage = "El nombre del representante legal es requerido")]
    [StringLength(300)]
    public string NombreRepresentanteLegal { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La identificación del representante es requerida")]
    [StringLength(50)]
    public string IdentificacionRepresentante { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string CargoRepresentante { get; set; } = "Representante Legal";
    
    [StringLength(300)]
    public string? NombreContador { get; set; }
    
    [StringLength(50)]
    public string? TarjetaProfesionalContador { get; set; }
    
    [StringLength(300)]
    public string? NombreRevisorFiscal { get; set; }
    
    [StringLength(50)]
    public string? TarjetaProfesionalRevisorFiscal { get; set; }
}

/// <summary>
/// DTO para anular un certificado.
/// </summary>
public class AnularCertificadoDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "La razón de anulación es requerida")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "La razón debe tener entre 10 y 1000 caracteres")]
    public string RazonAnulacion { get; set; } = string.Empty;
}

/// <summary>
/// Resultado paginado de certificados.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
