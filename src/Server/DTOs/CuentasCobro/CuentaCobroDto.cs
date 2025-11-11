namespace Server.DTOs.CuentasCobro;

/// <summary>
/// DTO para generar cuenta de cobro de un miembro deudor.
/// </summary>
public class CuentaCobroDto
{
    /// <summary>
    /// ID del miembro deudor.
    /// </summary>
    public Guid MiembroId { get; set; }
    
    /// <summary>
    /// Nombre completo del miembro.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de socio/miembro.
    /// </summary>
    public int NumeroSocio { get; set; }
    
    /// <summary>
    /// Fecha de ingreso del miembro.
    /// </summary>
    public DateOnly? FechaIngreso { get; set; }
    
    /// <summary>
    /// Email del miembro para notificaciones.
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Teléfono/celular del miembro.
    /// </summary>
    public string? Telefono { get; set; }
    
    /// <summary>
    /// Lista de meses adeudados con su detalle.
    /// </summary>
    public List<ItemCuentaCobro> Items { get; set; } = new();
    
    /// <summary>
    /// Total adeudado en COP.
    /// </summary>
    public decimal TotalCop { get; set; }
    
    /// <summary>
    /// Fecha de generación de la cuenta de cobro.
    /// </summary>
    public DateTime FechaGeneracion { get; set; }
    
    /// <summary>
    /// Número consecutivo de la cuenta de cobro (opcional).
    /// </summary>
    public string? Consecutivo { get; set; }
}

/// <summary>
/// Item individual de la cuenta de cobro (representa un mes o concepto adeudado).
/// </summary>
public class ItemCuentaCobro
{
    /// <summary>
    /// Descripción del ítem (ej: "Mensualidad Enero 2025").
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;
    
    /// <summary>
    /// Cantidad (usualmente 1 para mensualidades).
    /// </summary>
    public int Cantidad { get; set; } = 1;
    
    /// <summary>
    /// Precio unitario en COP.
    /// </summary>
    public decimal PrecioUnitarioCop { get; set; }
    
    /// <summary>
    /// Subtotal = Cantidad × PrecioUnitarioCop.
    /// </summary>
    public decimal SubtotalCop { get; set; }
    
    /// <summary>
    /// Mes al que corresponde (para ordenamiento y referencia).
    /// </summary>
    public DateOnly? MesReferencia { get; set; }
}
