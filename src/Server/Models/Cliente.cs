using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa un cliente externo (no miembro) en el sistema
/// </summary>
public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Identificacion { get; set; }

    /// <summary>
    /// Tipo: Persona Natural, Empresa
    /// </summary>
    [MaxLength(50)]
    public string Tipo { get; set; } = "Persona Natural";

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    /// <summary>
    /// Límite de crédito en COP
    /// </summary>
    public decimal? LimiteCredito { get; set; }

    /// <summary>
    /// Días de crédito otorgados
    /// </summary>
    public int DiasCredito { get; set; } = 0;

    /// <summary>
    /// Puntos de fidelización acumulados
    /// </summary>
    public int PuntosFidelizacion { get; set; } = 0;

    [MaxLength(500)]
    public string? Notas { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    // Navegación
    public virtual ICollection<VentaProducto> Ventas { get; set; } = new List<VentaProducto>();
}
