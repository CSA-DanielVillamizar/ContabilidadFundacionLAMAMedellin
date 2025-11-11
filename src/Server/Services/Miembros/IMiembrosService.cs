using Server.DTOs;
using Server.DTOs.Miembros;
using Server.Models;

namespace Server.Services.Miembros;

/// <summary>
/// Servicio de consulta y gestión de miembros.
/// </summary>
public interface IMiembrosService
{
    /// <summary>
    /// Obtiene un listado paginado de miembros con búsqueda y filtro por estado.
    /// </summary>
    /// <param name="query">Texto de búsqueda por nombre o documento.</param>
    /// <param name="estado">Estado a filtrar; null para todos.</param>
    /// <param name="page">Página (1-indexed).</param>
    /// <param name="pageSize">Tamaño de página.</param>
    /// <returns>Resultado paginado de <see cref="MiembroListItem"/>.</returns>
    Task<PagedResult<MiembroListItem>> GetPagedAsync(string? query, EstadoMiembro? estado, int page = 1, int pageSize = 10);

    /// <summary>
    /// Obtiene un miembro por ID.
    /// </summary>
    /// <param name="id">ID del miembro.</param>
    /// <returns>Detalle del miembro o null si no existe.</returns>
    Task<MiembroDetailDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Crea un nuevo miembro.
    /// </summary>
    /// <param name="dto">Datos del miembro a crear.</param>
    /// <param name="currentUser">Usuario que crea el miembro.</param>
    /// <returns>ID del miembro creado.</returns>
    Task<Guid> CreateAsync(CreateMiembroDto dto, string currentUser = "system");

    /// <summary>
    /// Actualiza un miembro existente.
    /// </summary>
    /// <param name="dto">Datos actualizados del miembro.</param>
    /// <param name="currentUser">Usuario que actualiza el miembro.</param>
    /// <returns>True si se actualizó correctamente, false si no existe.</returns>
    Task<bool> UpdateAsync(UpdateMiembroDto dto, string currentUser = "system");

    /// <summary>
    /// Elimina un miembro.
    /// </summary>
    /// <param name="id">ID del miembro a eliminar.</param>
    /// <returns>True si se eliminó correctamente, false si no existe.</returns>
    Task<bool> DeleteAsync(Guid id);
}
