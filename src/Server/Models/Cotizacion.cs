using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa una cotización antes de convertirse en venta
/// </summary>
public class Cotizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string NumeroCotizacion { get; set; } = string.Empty;

    public DateTime FechaCotizacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Cliente externo
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// O miembro
    /// </summary>
    public Guid? MiembroId { get; set; }

    [MaxLength(100)]
    public string? NombreCliente { get; set; }

    [MaxLength(100)]
    public string? EmailCliente { get; set; }

    [MaxLength(50)]
    public string? TelefonoCliente { get; set; }

    public decimal TotalCOP { get; set; }

    public decimal? TotalUSD { get; set; }

    public decimal SubtotalCOP { get; set; }

    public decimal DescuentoCOP { get; set; }

    /// <summary>
    /// Estado: Pendiente, Aprobada, Rechazada, Convertida, Vencida
    /// </summary>
    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID de la venta generada si se aprobó
    /// </summary>
    public Guid? VentaId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual Cliente? Cliente { get; set; }
    public virtual Miembro? Miembro { get; set; }
    public virtual VentaProducto? Venta { get; set; }
    public virtual ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}

/// <summary>
/// Detalle de productos en una cotización
/// </summary>
public class DetalleCotizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CotizacionId { get; set; }

    public Guid ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitarioCOP { get; set; }

    public decimal DescuentoCOP { get; set; } = 0;

    public decimal SubtotalCOP { get; set; }

    [MaxLength(500)]
    public string? Notas { get; set; }

    // Navegación
    public virtual Cotizacion Cotizacion { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}
