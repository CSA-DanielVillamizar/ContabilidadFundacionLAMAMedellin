using System.ComponentModel.DataAnnotations;
using Server.Models;

namespace Server.DTOs.Miembros;

/// <summary>
/// DTO para crear un nuevo miembro.
/// </summary>
public class CreateMiembroDto
{
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(250, ErrorMessage = "El nombre no puede exceder 250 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(120)]
    public string Nombres { get; set; } = string.Empty;

    [StringLength(120)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es requerido")]
    [StringLength(40)]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Celular { get; set; }

    [StringLength(500)]
    public string? Direccion { get; set; }

    public int? NumeroSocio { get; set; }

    [StringLength(100)]
    public string? Cargo { get; set; }

    [StringLength(50)]
    public string? Rango { get; set; }

    public EstadoMiembro Estado { get; set; } = EstadoMiembro.Activo;

    public DateOnly? FechaIngreso { get; set; }
}

/// <summary>
/// DTO para editar un miembro existente.
/// </summary>
public class UpdateMiembroDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(250, ErrorMessage = "El nombre no puede exceder 250 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(120)]
    public string Nombres { get; set; } = string.Empty;

    [StringLength(120)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es requerido")]
    [StringLength(40)]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Celular { get; set; }

    [StringLength(500)]
    public string? Direccion { get; set; }

    public int? NumeroSocio { get; set; }

    [StringLength(100)]
    public string? Cargo { get; set; }

    [StringLength(50)]
    public string? Rango { get; set; }

    public EstadoMiembro Estado { get; set; }

    public DateOnly? FechaIngreso { get; set; }
}

/// <summary>
/// DTO detallado de un miembro individual.
/// </summary>
public class MiembroDetailDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? Direccion { get; set; }
    public int? NumeroSocio { get; set; }
    public string? Cargo { get; set; }
    public string? Rango { get; set; }
    public EstadoMiembro Estado { get; set; }
    public DateOnly? FechaIngreso { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
