using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa un ingreso de dinero a la fundación (complemento de Recibo)
/// </summary>
public class Ingreso
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Número consecutivo del ingreso
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NumeroIngreso { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del ingreso
    /// </summary>
    [Required]
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Categoría del ingreso (Mensualidad, Ventas, Donaciones, etc.)
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingreso
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Valor del ingreso en COP
    /// </summary>
    [Required]
    public decimal ValorCop { get; set; }

    /// <summary>
    /// Método de pago del ingreso
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MetodoPago { get; set; } = string.Empty;

    /// <summary>
    /// Referencia de la transacción (número de transferencia, etc.)
    /// </summary>
    [MaxLength(200)]
    public string? ReferenciaTransaccion { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Hash determinístico para deduplicación de imports (SHA256 de Fecha|Valor|Descripción|Categoría|MetodoPago|NumeroIngreso).
    /// Permite idempotencia: el import puede ejecutarse múltiples veces sin crear duplicados.
    /// </summary>
    [MaxLength(64)]
    public string? ImportRowHash { get; set; }
}
