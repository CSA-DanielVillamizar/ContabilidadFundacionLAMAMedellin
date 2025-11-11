namespace Server.DTOs.Inventario;

/// <summary>
/// DTO para visualización de movimientos de inventario
/// </summary>
public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    public string NumeroMovimiento { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid? CompraId { get; set; }
    public Guid? VentaId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear movimiento manual de inventario (ajustes)
/// </summary>
public class MovimientoInventarioCreateDto
{
    public int Tipo { get; set; }
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}
