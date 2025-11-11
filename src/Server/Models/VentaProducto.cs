using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa una venta de productos a miembros locales, otros capítulos o externos
/// </summary>
public class VentaProducto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Número único de la venta (ej: VENT-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NumeroVenta { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se realizó la venta
    /// </summary>
    [Required]
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tipo de cliente
    /// </summary>
    [Required]
    public TipoCliente TipoCliente { get; set; }

    /// <summary>
    /// ID del miembro si TipoCliente = MiembroLocal
    /// </summary>
    public Guid? MiembroId { get; set; }

    /// <summary>
    /// ID del cliente externo (si aplica)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente si es OtroCapitulo o Externo
    /// </summary>
    [MaxLength(200)]
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Identificación del cliente externo
    /// </summary>
    [MaxLength(50)]
    public string? IdentificacionCliente { get; set; }

    /// <summary>
    /// Email del cliente para envío de recibo
    /// </summary>
    [MaxLength(100)]
    public string? EmailCliente { get; set; }

    /// <summary>
    /// Total de la venta en COP
    /// </summary>
    [Required]
    public decimal TotalCOP { get; set; }

    /// <summary>
    /// Total de la venta en USD (si aplica)
    /// </summary>
    public decimal? TotalUSD { get; set; }

    /// <summary>
    /// Método de pago
    /// </summary>
    [Required]
    public MetodoPagoVenta MetodoPago { get; set; }

    /// <summary>
    /// Estado de la venta
    /// </summary>
    [Required]
    public EstadoVenta Estado { get; set; } = EstadoVenta.Pendiente;

    /// <summary>
    /// ID del recibo generado automáticamente
    /// </summary>
    public Guid? ReciboId { get; set; }

    /// <summary>
    /// ID del ingreso vinculado (cuando se registra el pago)
    /// </summary>
    public Guid? IngresoId { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual Miembro? Miembro { get; set; }
    public virtual Cliente? Cliente { get; set; }
    public virtual Recibo? Recibo { get; set; }
    public virtual Ingreso? Ingreso { get; set; }
    public virtual ICollection<DetalleVentaProducto> Detalles { get; set; } = new List<DetalleVentaProducto>();
}

/// <summary>
/// Detalle de productos en una venta
/// </summary>
public class DetalleVentaProducto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VentaId { get; set; }

    [Required]
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad de unidades vendidas
    /// </summary>
    [Required]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario de venta en COP
    /// </summary>
    [Required]
    public decimal PrecioUnitarioCOP { get; set; }

    /// <summary>
    /// Descuento aplicado (si aplica)
    /// </summary>
    public decimal? DescuentoCOP { get; set; }

    /// <summary>
    /// Subtotal = (Cantidad * PrecioUnitarioCOP) - Descuento
    /// </summary>
    [Required]
    public decimal SubtotalCOP { get; set; }

    /// <summary>
    /// Notas específicas del item
    /// </summary>
    [MaxLength(500)]
    public string? Notas { get; set; }

    // Navegación
    public virtual VentaProducto Venta { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}

/// <summary>
/// Tipos de cliente para ventas
/// </summary>
public enum TipoCliente
{
    MiembroLocal = 1,       // Miembro del capítulo Medellín
    OtroCapitulo = 2,       // Otro capítulo de LAMA
    Externo = 3             // Cliente externo (no LAMA)
}

/// <summary>
/// Métodos de pago aceptados
/// </summary>
public enum MetodoPagoVenta
{
    Efectivo = 1,
    Transferencia = 2,
    Tarjeta = 3,
    Nequi = 4,
    DaviPlata = 5,
    Otro = 6
}

/// <summary>
/// Estados posibles de una venta
/// </summary>
public enum EstadoVenta
{
    Pendiente = 1,          // Registrada pero no pagada
    Pagada = 2,             // Ya se recibió el pago
    Entregada = 3,          // Productos entregados al cliente
    Cancelada = 4           // Venta cancelada
}
