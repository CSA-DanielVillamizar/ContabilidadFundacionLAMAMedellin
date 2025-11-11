using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa el cierre contable de un mes específico.
/// Una vez cerrado, no se permiten ediciones en recibos ni egresos de ese período.
/// </summary>
public class CierreMensual
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Año del período cerrado (ej: 2025)
    /// </summary>
    [Required]
    public int Ano { get; set; }

    /// <summary>
    /// Mes del período cerrado (1-12)
    /// </summary>
    [Required]
    [Range(1, 12)]
    public int Mes { get; set; }

    /// <summary>
    /// Fecha y hora en que se realizó el cierre
    /// </summary>
    [Required]
    public DateTime FechaCierre { get; set; }

    /// <summary>
    /// Usuario que realizó el cierre
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string UsuarioCierre { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones o notas del cierre (opcional)
    /// </summary>
    [MaxLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Saldo inicial calculado al momento del cierre (para validación futura)
    /// </summary>
    public decimal SaldoInicialCalculado { get; set; }

    /// <summary>
    /// Total de ingresos del mes cerrado
    /// </summary>
    public decimal TotalIngresos { get; set; }

    /// <summary>
    /// Total de egresos del mes cerrado
    /// </summary>
    public decimal TotalEgresos { get; set; }

    /// <summary>
    /// Saldo final del mes cerrado (SaldoInicial + TotalIngresos - TotalEgresos)
    /// </summary>
    public decimal SaldoFinal { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
