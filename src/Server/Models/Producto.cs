using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa un producto disponible para venta (souvenirs, parches, camisetas, jerseys, etc.)
/// </summary>
public class Producto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Código único del producto (ej: PATCH-001, SOUV-002, CAM-XL-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del producto
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de producto
    /// </summary>
    [Required]
    public TipoProducto Tipo { get; set; }

    /// <summary>
    /// Precio de venta en COP
    /// </summary>
    [Required]
    public decimal PrecioVentaCOP { get; set; }

    /// <summary>
    /// Precio de venta en USD (opcional)
    /// </summary>
    public decimal? PrecioVentaUSD { get; set; }

    /// <summary>
    /// Cantidad actual en inventario
    /// </summary>
    [Required]
    public int StockActual { get; set; } = 0;

    /// <summary>
    /// Stock mínimo para generar alerta de reorden
    /// </summary>
    [Required]
    public int StockMinimo { get; set; } = 5;

    /// <summary>
    /// Talla (aplica para camisetas y jerseys)
    /// </summary>
    [MaxLength(10)]
    public string? Talla { get; set; }

    /// <summary>
    /// Descripción detallada del producto
    /// </summary>
    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si es un parche oficial que se adquiere de LAMA International
    /// </summary>
    public bool EsParcheOficial { get; set; } = false;

    /// <summary>
    /// URL de imagen del producto (opcional)
    /// </summary>
    [MaxLength(500)]
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Indica si el producto está activo para venta
    /// </summary>
    public bool Activo { get; set; } = true;

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual ICollection<DetalleCompraProducto> DetallesCompra { get; set; } = new List<DetalleCompraProducto>();
    public virtual ICollection<DetalleVentaProducto> DetallesVenta { get; set; } = new List<DetalleVentaProducto>();
    public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
}

/// <summary>
/// Tipos de productos disponibles
/// </summary>
public enum TipoProducto
{
    Parche = 1,
    Souvenir = 2,
    Camiseta = 3,
    Jersey = 4,
    Gorra = 5,
    Sticker = 6,
    Llavero = 7,
    Otros = 99
}
