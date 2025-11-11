using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa una compra de productos a proveedores (LAMA International, etc.)
/// </summary>
public class CompraProducto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Número único de la compra (ej: COMP-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NumeroCompra { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se realizó la compra
    /// </summary>
    [Required]
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID del proveedor (relación con tabla Proveedores)
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor (ej: "LAMA International", "Confecciones XYZ")
    /// Mantiene compatibilidad con texto libre si no hay ProveedorId
    /// </summary>
    [MaxLength(200)]
    public string? Proveedor { get; set; }

    /// <summary>
    /// Total de la compra en COP
    /// </summary>
    [Required]
    public decimal TotalCOP { get; set; }

    /// <summary>
    /// Total de la compra en USD (si aplica)
    /// </summary>
    public decimal? TotalUSD { get; set; }

    /// <summary>
    /// TRM aplicada si hay conversión USD -> COP
    /// </summary>
    public decimal? TrmAplicada { get; set; }

    /// <summary>
    /// Estado de la compra
    /// </summary>
    [Required]
    public EstadoCompra Estado { get; set; } = EstadoCompra.Pendiente;

    /// <summary>
    /// Número de factura o documento del proveedor
    /// </summary>
    [MaxLength(100)]
    public string? NumeroFacturaProveedor { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del egreso vinculado (cuando se paga la compra)
    /// </summary>
    public Guid? EgresoId { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual Proveedor? ProveedorObj { get; set; }
    public virtual Egreso? Egreso { get; set; }
    public virtual ICollection<DetalleCompraProducto> Detalles { get; set; } = new List<DetalleCompraProducto>();
}

/// <summary>
/// Detalle de productos en una compra
/// </summary>
public class DetalleCompraProducto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CompraId { get; set; }

    [Required]
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad de unidades compradas
    /// </summary>
    [Required]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario de compra en COP
    /// </summary>
    [Required]
    public decimal PrecioUnitarioCOP { get; set; }

    /// <summary>
    /// Subtotal = Cantidad * PrecioUnitarioCOP
    /// </summary>
    [Required]
    public decimal SubtotalCOP { get; set; }

    /// <summary>
    /// Notas específicas del item
    /// </summary>
    [MaxLength(500)]
    public string? Notas { get; set; }

    // Navegación
    public virtual CompraProducto Compra { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}

/// <summary>
/// Estados posibles de una compra
/// </summary>
public enum EstadoCompra
{
    Pendiente = 1,      // Registrada pero no pagada
    Pagada = 2,         // Ya se generó el egreso
    Recibida = 3,       // Productos recibidos e inventariados
    Cancelada = 4       // Compra cancelada
}
