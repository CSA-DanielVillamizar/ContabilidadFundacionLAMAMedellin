using Microsoft.AspNetCore.Http;
using Server.Models;

namespace Server.Services.Egresos;

/// <summary>
/// Servicio de dominio para gestionar egresos (CRUD) y sus adjuntos de soporte.
/// </summary>
public interface IEgresosService
{
    /// <summary>
    /// Crea un egreso con datos suministrados y opcionalmente un archivo de soporte.
    /// </summary>
    Task<Egreso> CrearAsync(Egreso egreso, IFormFile? soporte, string usuarioActual, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un egreso por Id.
    /// </summary>
    Task<Egreso?> ObtenerAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lista egresos con filtros opcionales por fecha y categoría.
    /// </summary>
    Task<List<Egreso>> ListarAsync(DateTime? desde, DateTime? hasta, string? categoria, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un egreso y opcionalmente reemplaza el archivo de soporte.
    /// </summary>
    Task<Egreso?> ActualizarAsync(Guid id, Egreso egreso, IFormFile? soporte, string usuarioActual, CancellationToken ct = default);

    /// <summary>
    /// Elimina un egreso y su soporte si existe.
    /// </summary>
    Task<bool> EliminarAsync(Guid id, CancellationToken ct = default);

    // Soporte para uso desde componentes Blazor Server (sin HttpClient hacia la misma API)
    Task<Egreso> CrearAsync(Egreso egreso, Stream? soporteStream, string? soporteFileName, string usuarioActual, CancellationToken ct = default);
    Task<Egreso?> ActualizarAsync(Guid id, Egreso egreso, Stream? soporteStream, string? soporteFileName, string usuarioActual, CancellationToken ct = default);
}
