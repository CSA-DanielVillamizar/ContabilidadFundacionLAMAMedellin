namespace Server.DTOs.CuentasCobro;

/// <summary>
/// Solicitud para generar una cuenta de cobro personalizada con items arbitrarios.
/// </summary>
public class CuentaCobroCustomRequestDto
{
    /// <summary>
    /// ID del miembro al que se le generará la cuenta de cobro.
    /// </summary>
    public Guid MiembroId { get; set; }

    /// <summary>
    /// Lista de items (conceptos) a cobrar.
    /// </summary>
    public List<CuentaCobroCustomItem> Items { get; set; } = new();

    /// <summary>
    /// Consecutivo opcional a mostrar en el PDF.
    /// </summary>
    public string? Consecutivo { get; set; }

    /// <summary>
    /// Observaciones opcionales.
    /// </summary>
    public string? Observaciones { get; set; }
}

/// <summary>
/// Item de la cuenta de cobro personalizada.
/// </summary>
public class CuentaCobroCustomItem
{
    /// <summary>
    /// Descripción del concepto.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del concepto.
    /// </summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>
    /// Precio unitario en COP.
    /// </summary>
    public decimal PrecioUnitarioCop { get; set; }

    /// <summary>
    /// Subtotal calculado (Cantidad × PrecioUnitarioCop). Si no se provee, el servicio lo calculará.
    /// </summary>
    public decimal? SubtotalCop { get; set; }
}