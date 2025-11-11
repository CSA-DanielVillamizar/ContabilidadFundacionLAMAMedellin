using Server.DTOs.Inventario;

namespace Server.Services.Inventario;

/// <summary>
/// Interface para el servicio de gestión de inventario
/// </summary>
public interface IInventarioService
{
    Task<List<MovimientoInventarioDto>> GetAllMovimientosAsync();
    Task<List<MovimientoInventarioDto>> GetMovimientosByProductoAsync(Guid productoId);
    Task<List<MovimientoInventarioDto>> GetMovimientosByTipoAsync(int tipo);
    Task<List<MovimientoInventarioDto>> GetMovimientosByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<MovimientoInventarioDto?> GetMovimientoByIdAsync(Guid id);
    Task<MovimientoInventarioDto> CreateMovimientoManualAsync(MovimientoInventarioCreateDto dto, string? createdBy = null);
}
