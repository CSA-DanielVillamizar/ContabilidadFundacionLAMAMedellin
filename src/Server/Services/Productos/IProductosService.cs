using Server.DTOs.Productos;

namespace Server.Services.Productos;

/// <summary>
/// Interface para el servicio de gestión de productos
/// </summary>
public interface IProductosService
{
    Task<List<ProductoDto>> GetAllAsync();
    Task<List<ProductoDto>> GetActivosAsync();
    Task<List<ProductoDto>> GetBajoStockAsync();
    Task<ProductoDto?> GetByIdAsync(Guid id);
    Task<ProductoDto?> GetByCodigoAsync(string codigo);
    Task<ProductoDto> CreateAsync(ProductoCreateUpdateDto dto, string? createdBy = null);
    Task<ProductoDto> UpdateAsync(Guid id, ProductoCreateUpdateDto dto, string? updatedBy = null);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ActivarDesactivarAsync(Guid id, bool activo, string? updatedBy = null);
    Task<int> AjustarStockAsync(Guid productoId, int nuevaCantidad, string motivo, string? createdBy = null);
}
