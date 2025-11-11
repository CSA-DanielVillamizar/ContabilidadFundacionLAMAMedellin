namespace Server.DTOs.Ventas;

/// <summary>
/// DTO para visualización de ventas
/// </summary>
public class VentaProductoDto
{
    public Guid Id { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
    public string TipoCliente { get; set; } = string.Empty;
    public Guid? MiembroId { get; set; }
    public string? NombreMiembro { get; set; }
    public string? NombreCliente { get; set; }
    public string? IdentificacionCliente { get; set; }
    public string? EmailCliente { get; set; }
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Guid? ReciboId { get; set; }
    public Guid? IngresoId { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DetalleVentaProductoDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle de venta
/// </summary>
public class DetalleVentaProductoDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public decimal? DescuentoCOP { get; set; }
    public decimal SubtotalCOP { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para crear una venta
/// </summary>
public class VentaProductoCreateDto
{
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public int TipoCliente { get; set; }
    public Guid? MiembroId { get; set; }
    public string? NombreCliente { get; set; }
    public string? IdentificacionCliente { get; set; }
    public string? EmailCliente { get; set; }
    public decimal? TotalUSD { get; set; }
    public int MetodoPago { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle de venta al crear
/// </summary>
public class DetalleVentaCreateDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public decimal? DescuentoCOP { get; set; }
    public string? Notas { get; set; }
}
