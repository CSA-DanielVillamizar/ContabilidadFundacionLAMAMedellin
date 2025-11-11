using Server.Models;

namespace Server.DTOs.Miembros;

/// <summary>
/// DTO de listado de miembros con campos básicos para grilla.
/// </summary>
public class MiembroListItem
{
    public Guid Id { get; set; }
    public int? NumeroSocio { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public EstadoMiembro Estado { get; set; }
}
