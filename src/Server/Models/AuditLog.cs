namespace Server.Models;

/// <summary>
/// Modelo para registrar eventos de auditoría en el sistema.
/// Permite rastrear cambios importantes en entidades críticas.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tipo de entidad auditada (Recibo, Certificado, Miembro, etc.)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID de la entidad auditada
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Acción realizada (Created, Updated, Deleted, Emitted, Annulled, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Usuario que realizó la acción
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha y hora de la acción
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Valores anteriores (JSON) - opcional
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// Valores nuevos (JSON) - opcional
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Dirección IP del usuario - opcional
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Información adicional - opcional
    /// </summary>
    public string? AdditionalInfo { get; set; }
}
