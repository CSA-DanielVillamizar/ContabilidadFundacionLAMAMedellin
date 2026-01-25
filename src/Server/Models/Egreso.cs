using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa un egreso de tesorería (gasto) en COP.
/// </summary>
public class Egreso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Categoria { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal ValorCop { get; set; }
    public string? SoporteUrl { get; set; }
    public string UsuarioRegistro { get; set; } = string.Empty;
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";

    /// <summary>
    /// Hash determinístico para deduplicación de imports (SHA256 de Fecha|Valor|Descripción|Categoría|Proveedor).
    /// Permite idempotencia: el import puede ejecutarse múltiples veces sin crear duplicados.
    /// </summary>
    [MaxLength(64)]
    public string? ImportRowHash { get; set; }
}
