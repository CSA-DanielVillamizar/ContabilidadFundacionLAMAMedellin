using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.DTOs.Miembros;
using Server.Models;
using Server.Services.Audit;

namespace Server.Services.Miembros;

/// <summary>
/// Implementación de <see cref="IMiembrosService"/> basada en EF Core.
/// </summary>
public class MiembrosService : IMiembrosService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;

    public MiembrosService(AppDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    /// <inheritdoc />
    public async Task<PagedResult<MiembroListItem>> GetPagedAsync(string? query, EstadoMiembro? estado, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var q = _db.Miembros.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var txt = query.Trim();
            q = q.Where(m => m.NombreCompleto.Contains(txt) || m.Nombres.Contains(txt) || m.Apellidos.Contains(txt) || m.Documento.Contains(txt) || m.Cedula.Contains(txt));
        }

        if (estado.HasValue)
        {
            q = q.Where(m => m.Estado == estado.Value);
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderBy(m => m.NombreCompleto)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MiembroListItem
            {
                Id = m.Id,
                NumeroSocio = m.NumeroSocio ?? m.MemberNumber,
                NombreCompleto = m.NombreCompleto,
                Documento = string.IsNullOrWhiteSpace(m.Cedula) ? m.Documento : m.Cedula,
                Email = m.Email,
                Cargo = m.Cargo,
                Estado = m.Estado
            })
            .ToListAsync();

        return new PagedResult<MiembroListItem>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<MiembroDetailDto?> GetByIdAsync(Guid id)
    {
        var miembro = await _db.Miembros
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MiembroDetailDto
            {
                Id = m.Id,
                NombreCompleto = m.NombreCompleto,
                Nombres = m.Nombres,
                Apellidos = m.Apellidos,
                Cedula = m.Cedula,
                Email = m.Email,
                Celular = m.Celular,
                Direccion = m.Direccion,
                NumeroSocio = m.NumeroSocio,
                Cargo = m.Cargo,
                Rango = m.Rango,
                Estado = m.Estado,
                FechaIngreso = m.FechaIngreso,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy,
                UpdatedAt = m.UpdatedAt,
                UpdatedBy = m.UpdatedBy
            })
            .FirstOrDefaultAsync();

        return miembro;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateMiembroDto dto, string currentUser = "system")
    {
        var miembro = new Miembro
        {
            Id = Guid.NewGuid(),
            NombreCompleto = dto.NombreCompleto,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Cedula = dto.Cedula,
            Documento = dto.Cedula, // Alias
            Email = dto.Email,
            Celular = dto.Celular ?? string.Empty,
            Telefono = dto.Celular ?? string.Empty, // Alias
            Direccion = dto.Direccion ?? string.Empty,
            NumeroSocio = dto.NumeroSocio,
            MemberNumber = dto.NumeroSocio, // Alias
            Cargo = dto.Cargo ?? string.Empty,
            Rango = dto.Rango ?? string.Empty,
            Estado = dto.Estado,
            FechaIngreso = dto.FechaIngreso,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        _db.Miembros.Add(miembro);
        await _db.SaveChangesAsync();

        // ✅ AUDITORÍA: Registrar creación de miembro
        await _auditService.LogAsync(
            entityType: "Miembro",
            entityId: miembro.Id.ToString(),
            action: "MIEMBRO_CREADO",
            userName: currentUser,
            newValues: new
            {
                Nombre = miembro.NombreCompleto,
                Cedula = miembro.Cedula,
                Email = miembro.Email,
                Cargo = miembro.Cargo,
                Estado = miembro.Estado.ToString(),
                NumeroSocio = miembro.NumeroSocio
            },
            additionalInfo: $"Miembro creado: {miembro.NombreCompleto} (Número: {miembro.NumeroSocio}, Estado: {miembro.Estado})"
        );

        return miembro.Id;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UpdateMiembroDto dto, string currentUser = "system")
    {
        var miembro = await _db.Miembros.FindAsync(dto.Id);
        if (miembro == null) return false;

        // Capturar valores anteriores para auditoría
        var valoresAnteriores = $"Nombre: {miembro.NombreCompleto}, Cédula: {miembro.Cedula}, Email: {miembro.Email}, Cargo: {miembro.Cargo}, Estado: {miembro.Estado}";
        var estadoAnterior = miembro.Estado;

        miembro.NombreCompleto = dto.NombreCompleto;
        miembro.Nombres = dto.Nombres;
        miembro.Apellidos = dto.Apellidos;
        miembro.Cedula = dto.Cedula;
        miembro.Documento = dto.Cedula;
        miembro.Email = dto.Email;
        miembro.Celular = dto.Celular ?? string.Empty;
        miembro.Telefono = dto.Celular ?? string.Empty;
        miembro.Direccion = dto.Direccion ?? string.Empty;
        miembro.NumeroSocio = dto.NumeroSocio;
        miembro.MemberNumber = dto.NumeroSocio;
        miembro.Cargo = dto.Cargo ?? string.Empty;
        miembro.Rango = dto.Rango ?? string.Empty;
        miembro.Estado = dto.Estado;
        miembro.FechaIngreso = dto.FechaIngreso;
        miembro.UpdatedAt = DateTime.UtcNow;
        miembro.UpdatedBy = currentUser;

        await _db.SaveChangesAsync();

        // ✅ AUDITORÍA: Registrar actualización de miembro
        var accion = "MIEMBRO_ACTUALIZADO";
        var descripcion = $"Miembro actualizado: {miembro.NombreCompleto}";

        // Si cambió el estado, registrar evento específico
        if (estadoAnterior != miembro.Estado)
        {
            accion = "MIEMBRO_CAMBIO_ESTADO";
            descripcion = $"Cambio de estado de miembro: {miembro.NombreCompleto} ({estadoAnterior} → {miembro.Estado})";
        }

        await _auditService.LogAsync(
            entityType: "Miembro",
            entityId: miembro.Id.ToString(),
            action: accion,
            userName: currentUser,
            oldValues: new
            {
                Nombre = valoresAnteriores,
                Estado = estadoAnterior.ToString()
            },
            newValues: new
            {
                Nombre = miembro.NombreCompleto,
                Cedula = miembro.Cedula,
                Email = miembro.Email,
                Cargo = miembro.Cargo,
                Estado = miembro.Estado.ToString()
            },
            additionalInfo: descripcion
        );

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var miembro = await _db.Miembros.FindAsync(id);
        if (miembro == null) return false;

        // Capturar información antes de eliminar
        var nombreCompleto = miembro.NombreCompleto;
        var numeroSocio = miembro.NumeroSocio;
        var cedula = miembro.Cedula;

        _db.Miembros.Remove(miembro);
        await _db.SaveChangesAsync();

        // ✅ AUDITORÍA: Registrar eliminación de miembro
        await _auditService.LogAsync(
            entityType: "Miembro",
            entityId: id.ToString(),
            action: "MIEMBRO_ELIMINADO",
            userName: "system", // No tenemos currentUser en este método
            oldValues: new
            {
                Nombre = nombreCompleto,
                NumeroSocio = numeroSocio,
                Cedula = cedula
            },
            additionalInfo: $"Miembro eliminado: {nombreCompleto} (Número: {numeroSocio}, Cédula: {cedula})"
        );

        return true;
    }
}
