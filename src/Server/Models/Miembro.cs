namespace Server.Models;

/// <summary>
/// Representa un miembro del capítulo L.A.M.A. Medellín.
/// Campos básicos y auditoría. FechaIngreso usa DateOnly.
/// </summary>
public enum EstadoMiembro { Activo = 1, Inactivo = 2 }

public class Miembro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Información personal
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty; // Documento de identidad
    public string Documento { get; set; } = string.Empty; // Alias para Cedula (compatibilidad)
    
    // Contacto
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty; // Alias para Celular (compatibilidad)
    public string Direccion { get; set; } = string.Empty;
    
    // Información de membresía
    public int? NumeroSocio { get; set; } // Número de socio
    public int? MemberNumber { get; set; } // Alias para NumeroSocio (compatibilidad)
    public string Cargo { get; set; } = string.Empty; // SOCIO, TESORERO, PRESIDENTE, etc.
    public string Rango { get; set; } = string.Empty; // Full Color, Rockets, Prospecto
    public EstadoMiembro Estado { get; set; } = EstadoMiembro.Activo;
    public DateOnly? FechaIngreso { get; set; }

    public bool DatosIncompletos { get; set; } = false; // banderín para faltantes

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "seed";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
