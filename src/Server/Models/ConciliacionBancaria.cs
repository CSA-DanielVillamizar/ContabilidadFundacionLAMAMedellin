using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa una conciliación bancaria mensual
/// </summary>
public class ConciliacionBancaria
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Ano { get; set; }

    public int Mes { get; set; }

    /// <summary>
    /// Saldo según libros contables
    /// </summary>
    public decimal SaldoLibros { get; set; }

    /// <summary>
    /// Saldo según extracto bancario
    /// </summary>
    public decimal SaldoBanco { get; set; }

    /// <summary>
    /// Diferencia a conciliar
    /// </summary>
    public decimal Diferencia { get; set; }

    /// <summary>
    /// Estado: Pendiente, En Proceso, Conciliada
    /// </summary>
    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    public DateTime? FechaConciliacion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual ICollection<ItemConciliacion> Items { get; set; } = new List<ItemConciliacion>();
}

/// <summary>
/// Representa un ítem de diferencia en la conciliación
/// </summary>
public class ItemConciliacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConciliacionId { get; set; }

    /// <summary>
    /// Tipo: Cheque en tránsito, Depósito en tránsito, Error, Otro
    /// </summary>
    [MaxLength(100)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    /// <summary>
    /// true = suma, false = resta
    /// </summary>
    public bool EsSuma { get; set; } = true;

    public bool Conciliado { get; set; } = false;

    public DateTime? FechaConciliacion { get; set; }

    // Navegación
    public virtual ConciliacionBancaria Conciliacion { get; set; } = null!;
}
