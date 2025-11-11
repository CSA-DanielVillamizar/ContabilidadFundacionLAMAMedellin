using Server.DTOs.Ventas;

namespace Server.Services.Ventas;

/// <summary>
/// Interface para el servicio de gestión de ventas
/// </summary>
public interface IVentasService
{
    Task<List<VentaProductoDto>> GetAllAsync();
    Task<List<VentaProductoDto>> GetByEstadoAsync(int estado);
    Task<List<VentaProductoDto>> GetByMiembroAsync(Guid miembroId);
    Task<VentaProductoDto?> GetByIdAsync(Guid id);
    Task<VentaProductoDto> CreateAsync(VentaProductoCreateDto dto, string? createdBy = null);
    Task<bool> CambiarEstadoAsync(Guid id, int nuevoEstado, string? updatedBy = null);
    Task<bool> RegistrarPagoAsync(Guid ventaId, Guid ingresoId, string? updatedBy = null);
    Task<bool> RegistrarEntregaAsync(Guid ventaId, string? updatedBy = null);
}
