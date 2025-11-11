using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa un proveedor en el sistema de Gerencia de Negocios
/// </summary>
public class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Nit { get; set; }

    [MaxLength(100)]
    public string? ContactoNombre { get; set; }

    [MaxLength(50)]
    public string? ContactoTelefono { get; set; }

    [MaxLength(100)]
    public string? ContactoEmail { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    [MaxLength(50)]
    public string? Pais { get; set; }

    /// <summary>
    /// Términos de pago (ej: 30 días, contado, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? TerminosPago { get; set; }

    /// <summary>
    /// Días de crédito otorgados
    /// </summary>
    public int DiasCredito { get; set; } = 0;

    /// <summary>
    /// Calificación del proveedor (1-5)
    /// </summary>
    public int? Calificacion { get; set; }

    [MaxLength(500)]
    public string? Notas { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual ICollection<CompraProducto> Compras { get; set; } = new List<CompraProducto>();
}
