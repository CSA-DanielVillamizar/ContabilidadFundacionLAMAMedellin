using Server.DTOs.Proveedores;

namespace Server.Services.Proveedores;

/// <summary>
/// Servicio para gestión de proveedores
/// </summary>
public interface IProveedoresService
{
    /// <summary>
    /// Obtiene todos los proveedores con filtros y paginación
    /// </summary>
    Task<(List<ProveedorDto> Proveedores, int TotalCount)> ObtenerProveedoresAsync(
        string? busqueda = null,
        bool? activo = null,
        int pagina = 1,
        int registrosPorPagina = 20);

    /// <summary>
    /// Obtiene un proveedor por ID
    /// </summary>
    Task<ProveedorDto?> ObtenerProveedorPorIdAsync(Guid id);

    /// <summary>
    /// Obtiene el detalle completo del proveedor con historial
    /// </summary>
    Task<ProveedorDetalleDto?> ObtenerProveedorDetalleAsync(Guid id);

    /// <summary>
    /// Crea un nuevo proveedor
    /// </summary>
    Task<(bool Success, string Message, Guid? ProveedorId)> CrearProveedorAsync(
        ProveedorFormDto dto, 
        string usuarioId);

    /// <summary>
    /// Actualiza un proveedor existente
    /// </summary>
    Task<(bool Success, string Message)> ActualizarProveedorAsync(
        Guid id, 
        ProveedorFormDto dto, 
        string usuarioId);

    /// <summary>
    /// Elimina (desactiva) un proveedor
    /// </summary>
    Task<(bool Success, string Message)> EliminarProveedorAsync(Guid id, string usuarioId);

    /// <summary>
    /// Obtiene el historial de compras a un proveedor
    /// </summary>
    Task<List<CompraProveedorDto>> ObtenerHistorialComprasAsync(
        Guid proveedorId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null);

    /// <summary>
    /// Valida si un NIT ya existe
    /// </summary>
    Task<bool> ExisteNitAsync(string nit, Guid? proveedorIdExcluir = null);

    /// <summary>
    /// Obtiene proveedores activos para dropdown
    /// </summary>
    Task<List<ProveedorSimpleDto>> ObtenerProveedoresActivosAsync();
}

/// <summary>
/// DTO simplificado para dropdowns
/// </summary>
public class ProveedorSimpleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public int DiasCredito { get; set; }
}

/// <summary>
/// DTO para historial de compras
/// </summary>
public class CompraProveedorDto
{
    public Guid CompraId { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    public int TotalProductos { get; set; }
    public string? Observaciones { get; set; }
}
