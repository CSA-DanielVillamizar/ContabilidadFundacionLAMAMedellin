namespace Server.DTOs.Compras;

/// <summary>
/// DTO para visualización de compras
/// </summary>
public class CompraProductoDto
{
    public Guid Id { get; set; }
    public string NumeroCompra { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    public decimal? TrmAplicada { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? NumeroFacturaProveedor { get; set; }
    public string? Observaciones { get; set; }
    public Guid? EgresoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DetalleCompraProductoDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle de compra
/// </summary>
public class DetalleCompraProductoDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public decimal SubtotalCOP { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para crear una compra
/// </summary>
public class CompraProductoCreateDto
{
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// ID del proveedor seleccionado (opcional). Si se proporciona, se usará para vincular la compra al proveedor.
    /// </summary>
    public Guid? ProveedorId { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public decimal? TotalUSD { get; set; }
    public decimal? TrmAplicada { get; set; }
    public string? NumeroFacturaProveedor { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleCompraCreateDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle de compra al crear
/// </summary>
public class DetalleCompraCreateDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public string? Notas { get; set; }
}
