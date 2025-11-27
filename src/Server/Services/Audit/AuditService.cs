using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using System.Text.Json;

namespace Server.Services.Audit;

/// <summary>
/// Servicio para registrar eventos de auditoría en el sistema.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra un evento de auditoría.
    /// </summary>
    Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null);
    
    /// <summary>
    /// Obtiene los logs de auditoría de una entidad específica.
    /// </summary>
    Task<List<AuditLog>> GetEntityLogsAsync(string entityType, string entityId);
    
    /// <summary>
    /// Obtiene los logs de auditoría recientes.
    /// </summary>
    Task<List<AuditLog>> GetRecentLogsAsync(int count = 100);
}

/// <summary>
/// Implementación del servicio de auditoría.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityType, string entityId, string action, string userName, object? oldValues = null, object? newValues = null, string? additionalInfo = null)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            AdditionalInfo = additionalInfo
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetEntityLogsAsync(string entityType, string entityId)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetRecentLogsAsync(int count = 100)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
