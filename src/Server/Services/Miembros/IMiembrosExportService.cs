namespace Server.Services.Miembros;

/// <summary>
/// Servicio para exportar datos de miembros a diferentes formatos.
/// </summary>
public interface IMiembrosExportService
{
    /// <summary>
    /// Exporta el listado de miembros a Excel.
    /// </summary>
    /// <param name="query">Filtro de búsqueda opcional.</param>
    /// <param name="estado">Filtro por estado opcional.</param>
    /// <returns>Bytes del archivo Excel.</returns>
    Task<byte[]> ExportToExcelAsync(string? query, Server.Models.EstadoMiembro? estado);
}
