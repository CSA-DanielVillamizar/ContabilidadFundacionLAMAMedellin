namespace Server.DTOs.Notificaciones;

/// <summary>
/// DTO para visualización de notificaciones
/// </summary>
public class NotificacionDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Info, Alerta, Advertencia, Error, Exito
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLeida { get; set; }
    public string TiempoTranscurrido { get; set; } = string.Empty; // "Hace 5 minutos"
}

/// <summary>
/// DTO para crear notificaciones
/// </summary>
public class NotificacionCreateDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Info";
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Url { get; set; }
}

/// <summary>
/// DTO para respuesta con contador de no leídas
/// </summary>
public class NotificacionesResponseDto
{
    public List<NotificacionDto> Notificaciones { get; set; } = new();
    public int TotalNoLeidas { get; set; }
    public int TotalGeneral { get; set; }
    public bool TieneNuevas { get; set; }
}

/// <summary>
/// DTO para marcar notificaciones como leídas
/// </summary>
public class MarcarLeidasDto
{
    public List<Guid> NotificacionIds { get; set; } = new();
    public bool MarcarTodasLeidas { get; set; }
}
