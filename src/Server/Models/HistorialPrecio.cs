using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Historial de precios de un producto
/// </summary>
public class HistorialPrecio
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductoId { get; set; }

    public decimal PrecioAnteriorCOP { get; set; }

    public decimal PrecioNuevoCOP { get; set; }

    public decimal? PrecioAnteriorUSD { get; set; }

    public decimal? PrecioNuevoUSD { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CambiadoPor { get; set; } = string.Empty;

    // Navegación
    public virtual Producto Producto { get; set; } = null!;
}
