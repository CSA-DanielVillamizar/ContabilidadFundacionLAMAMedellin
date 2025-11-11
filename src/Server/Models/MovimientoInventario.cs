using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Registra todos los movimientos de inventario (entradas, salidas, ajustes)
/// </summary>
public class MovimientoInventario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Número consecutivo del movimiento
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NumeroMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    [Required]
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    [Required]
    public TipoMovimiento Tipo { get; set; }

    /// <summary>
    /// ID del producto afectado
    /// </summary>
    [Required]
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad del movimiento (positiva para entradas, negativa para salidas)
    /// </summary>
    [Required]
    public int Cantidad { get; set; }

    /// <summary>
    /// Stock antes del movimiento
    /// </summary>
    [Required]
    public int StockAnterior { get; set; }

    /// <summary>
    /// Stock después del movimiento
    /// </summary>
    [Required]
    public int StockNuevo { get; set; }

    /// <summary>
    /// ID de la compra que generó el movimiento (si aplica)
    /// </summary>
    public Guid? CompraId { get; set; }

    /// <summary>
    /// ID de la venta que generó el movimiento (si aplica)
    /// </summary>
    public Guid? VentaId { get; set; }

    /// <summary>
    /// Motivo del movimiento (especialmente para ajustes)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    // Navegación
    public virtual Producto Producto { get; set; } = null!;
    public virtual CompraProducto? Compra { get; set; }
    public virtual VentaProducto? Venta { get; set; }
}

/// <summary>
/// Tipos de movimiento de inventario
/// </summary>
public enum TipoMovimiento
{
    EntradaCompra = 1,          // Entrada por compra a proveedor
    SalidaVenta = 2,            // Salida por venta a cliente
    AjustePositivo = 3,         // Ajuste de inventario (aumento)
    AjusteNegativo = 4,         // Ajuste de inventario (disminución)
    DevolucionCliente = 5,      // Devolución de cliente (entrada)
    DevolucionProveedor = 6,    // Devolución a proveedor (salida)
    Merma = 7,                  // Pérdida por daño, robo, etc.
    Donacion = 8                // Salida por donación
}
