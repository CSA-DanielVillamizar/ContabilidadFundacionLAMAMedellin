using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs.Proveedores;

/// <summary>
/// DTO para visualización de proveedores en listados
/// </summary>
public class ProveedorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? Contacto { get; set; }
    public string? TerminosPago { get; set; }
    public int DiasCredito { get; set; }
    public decimal? Calificacion { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; }
    public int TotalCompras { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? ActualizadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para crear/editar proveedores
/// </summary>
public class ProveedorFormDto : INotifyPropertyChanged
{
    private string? _nit;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid? Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "El NIT no puede exceder 50 caracteres")]
    public string? Nit 
    { 
        get => _nit;
        set
        {
            if (_nit != value)
            {
                _nit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nit)));
            }
        }
    }
    
    [Required(ErrorMessage = "El contacto es obligatorio")]
    [StringLength(200, ErrorMessage = "El contacto no puede exceder 200 caracteres")]
    public string? Contacto { get; set; }
    
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
    public string? Telefono { get; set; }
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    [StringLength(200, ErrorMessage = "Los términos de pago no pueden exceder 200 caracteres")]
    public string? TerminosPago { get; set; }
    
    [Required(ErrorMessage = "Los días de crédito son obligatorios")]
    [Range(0, 365, ErrorMessage = "Los días de crédito deben estar entre 0 y 365")]
    public int DiasCredito { get; set; }
    
    [Range(0, 5, ErrorMessage = "La calificación debe estar entre 0 y 5")]
    public decimal? Calificacion { get; set; }
    
    public string? Notas { get; set; }
    
    public bool Activo { get; set; } = true;
    
    // Campos de auditoría (solo para edición)
    public string? CreadoPor { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? ActualizadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para detalle completo del proveedor con historial
/// </summary>
public class ProveedorDetalleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Contacto { get; set; }
    public string? TerminosPago { get; set; }
    public int DiasCredito { get; set; }
    public decimal? Calificacion { get; set; }
    public bool Activo { get; set; }
    
    // Estadísticas
    public int TotalCompras { get; set; }
    public decimal TotalCompradoCOP { get; set; }
    public decimal TotalCompradoUSD { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public DateTime? ProximoPago { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
