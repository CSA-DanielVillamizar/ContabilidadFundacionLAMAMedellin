using Server.DTOs.Compras;

namespace Server.Services.Compras;

/// <summary>
/// Interface para el servicio de gestión de compras
/// </summary>
public interface IComprasService
{
    Task<List<CompraProductoDto>> GetAllAsync();
    Task<List<CompraProductoDto>> GetByEstadoAsync(int estado);
    Task<CompraProductoDto?> GetByIdAsync(Guid id);
    Task<CompraProductoDto> CreateAsync(CompraProductoCreateDto dto, string? createdBy = null);
    Task<bool> CambiarEstadoAsync(Guid id, int nuevoEstado, string? updatedBy = null);
    Task<bool> RegistrarPagoAsync(Guid compraId, Guid egresoId, string? updatedBy = null);
    Task<bool> RegistrarRecepcionAsync(Guid compraId, string? updatedBy = null);
}
