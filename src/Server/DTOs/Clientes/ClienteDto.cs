namespace Server.DTOs.Clientes;

/// <summary>
/// DTO para visualización de clientes en listados
/// </summary>
public class ClienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Natural, Juridica
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? LimiteCredito { get; set; }
    public int PuntosFidelizacion { get; set; }
    public bool Activo { get; set; }
    public int TotalVentas { get; set; }
    public DateTime? UltimaVenta { get; set; }
    public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear/editar clientes
/// </summary>
public class ClienteFormDto
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Natural"; // Natural, Juridica
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal? LimiteCredito { get; set; }
    public int PuntosFidelizacion { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// DTO para detalle completo del cliente con historial
/// </summary>
public class ClienteDetalleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal? LimiteCredito { get; set; }
    public int PuntosFidelizacion { get; set; }
    public bool Activo { get; set; }
    
    // Estadísticas
    public int TotalVentas { get; set; }
    public decimal TotalCompradoCOP { get; set; }
    public decimal TotalCompradoUSD { get; set; }
    public DateTime? UltimaVenta { get; set; }
    public decimal PromedioCompra { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
