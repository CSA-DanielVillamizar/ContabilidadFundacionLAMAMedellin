namespace Server.DTOs.Productos;

/// <summary>
/// DTO para visualización de productos
/// </summary>
public class ProductoDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal PrecioVentaCOP { get; set; }
    public decimal? PrecioVentaUSD { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string? Talla { get; set; }
    public string? Descripcion { get; set; }
    public bool EsParcheOficial { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; }
    public bool BajoStock => StockActual <= StockMinimo;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear/actualizar productos
/// </summary>
public class ProductoCreateUpdateDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Tipo { get; set; }
    public decimal PrecioVentaCOP { get; set; }
    public decimal? PrecioVentaUSD { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; } = 5;
    public string? Talla { get; set; }
    public string? Descripcion { get; set; }
    public bool EsParcheOficial { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
}
