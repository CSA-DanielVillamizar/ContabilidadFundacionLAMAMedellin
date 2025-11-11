namespace Server.DTOs.HistorialPrecios;

/// <summary>
/// DTO para visualización del historial de precios
/// </summary>
public class HistorialPrecioDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    
    public DateTime FechaCambio { get; set; }
    
    // Precios anteriores
    public decimal? PrecioAnteriorCOP { get; set; }
    public decimal? PrecioAnteriorUSD { get; set; }
    
    // Precios nuevos
    public decimal? PrecioNuevoCOP { get; set; }
    public decimal? PrecioNuevoUSD { get; set; }
    
    // Cambios
    public decimal? DiferenciaCOP { get; set; }
    public decimal? DiferenciaUSD { get; set; }
    public decimal? PorcentajeCambioCOP { get; set; }
    public decimal? PorcentajeCambioUSD { get; set; }
    
    public string? Motivo { get; set; }
    public string? CambiadoPor { get; set; }
    public string? NombreUsuario { get; set; }
}

/// <summary>
/// DTO para crear registro de historial (automático)
/// </summary>
public class HistorialPrecioCreateDto
{
    public Guid ProductoId { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.Now;
    public decimal? PrecioAnteriorCOP { get; set; }
    public decimal? PrecioAnteriorUSD { get; set; }
    public decimal? PrecioNuevoCOP { get; set; }
    public decimal? PrecioNuevoUSD { get; set; }
    public string? Motivo { get; set; }
    public string? CambiadoPor { get; set; }
}

/// <summary>
/// DTO para análisis de variación de precios
/// </summary>
public class AnalisisPreciosDto
{
    public Guid ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    
    // Precios actuales
    public decimal? PrecioActualCOP { get; set; }
    public decimal? PrecioActualUSD { get; set; }
    
    // Estadísticas
    public int TotalCambios { get; set; }
    public DateTime? UltimoCambio { get; set; }
    public DateTime? PrimerRegistro { get; set; }
    
    // Variación acumulada
    public decimal? VariacionTotalCOP { get; set; }
    public decimal? VariacionTotalUSD { get; set; }
    public decimal? PorcentajeVariacionCOP { get; set; }
    public decimal? PorcentajeVariacionUSD { get; set; }
    
    // Precios mínimo/máximo histórico
    public decimal? PrecioMinimoCOP { get; set; }
    public decimal? PrecioMaximoCOP { get; set; }
    public decimal? PrecioMinimoUSD { get; set; }
    public decimal? PrecioMaximoUSD { get; set; }
    
    public List<HistorialPrecioDto> Historial { get; set; } = new();
}
