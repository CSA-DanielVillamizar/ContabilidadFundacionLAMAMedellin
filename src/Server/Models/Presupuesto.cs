using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa el presupuesto por concepto para un período
/// </summary>
public class Presupuesto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Ano { get; set; }

    public int Mes { get; set; }

    public int ConceptoId { get; set; }

    /// <summary>
    /// Monto presupuestado en COP
    /// </summary>
    [Required]
    public decimal MontoPresupuestado { get; set; }

    /// <summary>
    /// Monto ejecutado en COP (calculado)
    /// </summary>
    public decimal MontoEjecutado { get; set; } = 0;

    [MaxLength(500)]
    public string? Notas { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual Concepto Concepto { get; set; } = null!;
}
