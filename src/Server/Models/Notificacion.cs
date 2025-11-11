using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Representa una notificación en el sistema
/// </summary>
public class Notificacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo: Info, Advertencia, Error, Exito
    /// </summary>
    [MaxLength(50)]
    public string Tipo { get; set; } = "Info";

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// URL a la que redirige al hacer clic (opcional)
    /// </summary>
    [MaxLength(500)]
    public string? Url { get; set; }

    public bool Leida { get; set; } = false;

    public DateTime? FechaLeida { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ApplicationUser Usuario { get; set; } = null!;
}
