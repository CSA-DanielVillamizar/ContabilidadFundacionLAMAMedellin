using Server.DTOs.Cotizaciones;

namespace Server.Services.Cotizaciones;

/// <summary>
/// Contrato del servicio de gestión de cotizaciones.
/// Provee operaciones para CRUD, listado con filtros, cambio de estado y generación de numeración.
/// </summary>
public interface ICotizacionesService
{
    /// <summary>
    /// Obtiene cotizaciones filtradas y paginadas.
    /// </summary>
    Task<(List<CotizacionDto> Cotizaciones, int TotalCount)> ObtenerCotizacionesAsync(
        string? busqueda = null,
        string? estado = null,
        Guid? clienteId = null,
        Guid? miembroId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int pagina = 1,
        int registrosPorPagina = 20);

    /// <summary>
    /// Obtiene el detalle completo de una cotización.
    /// </summary>
    Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id);

    /// <summary>
    /// Crea una nueva cotización con sus detalles.
    /// </summary>
    Task<(bool Success, string Message, Guid? CotizacionId)> CrearCotizacionAsync(CotizacionFormDto dto, string usuarioId);

    /// <summary>
    /// Actualiza una cotización existente (solo si no está convertida o rechazada).
    /// </summary>
    Task<(bool Success, string Message)> ActualizarCotizacionAsync(Guid id, CotizacionFormDto dto, string usuarioId);

    /// <summary>
    /// Cambia el estado de una cotización respetando la lógica de transición.
    /// </summary>
    Task<(bool Success, string Message)> CambiarEstadoAsync(Guid id, string nuevoEstado, string usuarioId);

    /// <summary>
    /// Elimina una cotización (si no está convertida ni aprobada). Si ya se convirtió a venta, se bloquea.
    /// </summary>
    Task<(bool Success, string Message)> EliminarCotizacionAsync(Guid id, string usuarioId);

    /// <summary>
    /// Genera un número de cotización único con formato COT-YYYYMM-#####.
    /// </summary>
    Task<string> GenerarNumeroAsync(DateTime fechaCotizacion);
}