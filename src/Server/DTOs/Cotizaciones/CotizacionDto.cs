namespace Server.DTOs.Cotizaciones;

/// <summary>
/// DTO para visualización de cotizaciones en listados
/// </summary>
public class CotizacionDto
{
    public Guid Id { get; set; }
    public string NumeroCotizacion { get; set; } = string.Empty;
    public DateTime FechaCotizacion { get; set; }
    public DateTime FechaValidez { get; set; }
    
    // Cliente/Miembro
    public string TipoCliente { get; set; } = string.Empty; // Miembro, Externo
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public Guid? MiembroId { get; set; }
    public string? MiembroNombre { get; set; }
    
    // Totales
    public decimal SubtotalCOP { get; set; }
    public decimal DescuentoCOP { get; set; }
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    
    public string Estado { get; set; } = string.Empty; // Pendiente, Aprobada, Rechazada, Convertida
    public int TotalItems { get; set; }
    public bool Convertida { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear/editar cotizaciones
/// </summary>
public class CotizacionFormDto
{
    public Guid? Id { get; set; }
    public string? NumeroCotizacion { get; set; } // Auto-generado si es null
    public DateTime FechaCotizacion { get; set; } = DateTime.Now;
    public DateTime FechaValidez { get; set; } = DateTime.Now.AddDays(15);
    
    // Cliente/Miembro
    public string TipoCliente { get; set; } = "Miembro"; // Miembro, Externo
    public Guid? ClienteId { get; set; }
    public Guid? MiembroId { get; set; }
    
    // Totales
    public decimal SubtotalCOP { get; set; }
    public decimal DescuentoCOP { get; set; }
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";
    
    // Detalles
    public List<DetalleCotizacionFormDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle completo de cotización
/// </summary>
public class CotizacionDetalleDto
{
    public Guid Id { get; set; }
    public string NumeroCotizacion { get; set; } = string.Empty;
    public DateTime FechaCotizacion { get; set; }
    public DateTime FechaValidez { get; set; }
    
    // Cliente/Miembro
    public string TipoCliente { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteIdentificacion { get; set; }
    public string? ClienteEmail { get; set; }
    public Guid? MiembroId { get; set; }
    public string? MiembroNombre { get; set; }
    public string? MiembroEmail { get; set; }
    
    // Totales
    public decimal SubtotalCOP { get; set; }
    public decimal DescuentoCOP { get; set; }
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
    
    // Conversión
    public Guid? VentaId { get; set; }
    public string? NumeroVenta { get; set; }
    public DateTime? FechaConversion { get; set; }
    
    // Detalles
    public List<DetalleCotizacionDto> Detalles { get; set; } = new();
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO para detalle de línea de cotización
/// </summary>
public class DetalleCotizacionDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public decimal DescuentoCOP { get; set; }
    public decimal SubtotalCOP { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para crear/editar línea de cotización
/// </summary>
public class DetalleCotizacionFormDto
{
    public Guid? Id { get; set; }
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioCOP { get; set; }
    public decimal DescuentoCOP { get; set; }
    public decimal SubtotalCOP { get; set; }
    public string? Notas { get; set; }
}
