using Server.DTOs.Clientes;

namespace Server.Services.Clientes;

/// <summary>
/// Servicio para gestión de clientes
/// </summary>
public interface IClientesService
{
    /// <summary>
    /// Obtiene todos los clientes con filtros y paginación
    /// </summary>
    Task<(List<ClienteDto> Clientes, int TotalCount)> ObtenerClientesAsync(
        string? busqueda = null,
        bool? activo = null,
        string? tipo = null,
        int pagina = 1,
        int registrosPorPagina = 20);

    /// <summary>
    /// Obtiene un cliente por ID
    /// </summary>
    Task<ClienteDto?> ObtenerClientePorIdAsync(Guid id);

    /// <summary>
    /// Obtiene el detalle completo del cliente con historial
    /// </summary>
    Task<ClienteDetalleDto?> ObtenerClienteDetalleAsync(Guid id);

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    Task<(bool Success, string Message, Guid? ClienteId)> CrearClienteAsync(
        ClienteFormDto dto, 
        string usuarioId);

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    Task<(bool Success, string Message)> ActualizarClienteAsync(
        Guid id, 
        ClienteFormDto dto, 
        string usuarioId);

    /// <summary>
    /// Elimina (desactiva) un cliente
    /// </summary>
    Task<(bool Success, string Message)> EliminarClienteAsync(Guid id, string usuarioId);

    /// <summary>
    /// Obtiene el historial de ventas de un cliente
    /// </summary>
    Task<List<VentaClienteDto>> ObtenerHistorialVentasAsync(
        Guid clienteId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null);

    /// <summary>
    /// Valida si una identificación ya existe
    /// </summary>
    Task<bool> ExisteIdentificacionAsync(string identificacion, Guid? clienteIdExcluir = null);

    /// <summary>
    /// Obtiene clientes activos para dropdown
    /// </summary>
    Task<List<ClienteSimpleDto>> ObtenerClientesActivosAsync();
}

/// <summary>
/// DTO simplificado para dropdowns
/// </summary>
public class ClienteSimpleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal? LimiteCredito { get; set; }
}

/// <summary>
/// DTO para historial de ventas
/// </summary>
public class VentaClienteDto
{
    public Guid VentaId { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal TotalCOP { get; set; }
    public decimal? TotalUSD { get; set; }
    public int TotalProductos { get; set; }
    public string? Observaciones { get; set; }
}
