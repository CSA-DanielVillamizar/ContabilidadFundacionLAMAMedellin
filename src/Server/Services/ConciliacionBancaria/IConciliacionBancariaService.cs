using Server.DTOs.ConciliacionBancaria;

namespace Server.Services.ConciliacionBancaria;

/// <summary>
/// Interfaz del servicio de conciliación bancaria
/// </summary>
public interface IConciliacionBancariaService
{
    /// <summary>
    /// Obtiene listado de conciliaciones con filtros y paginación
    /// </summary>
    Task<(List<ConciliacionBancariaDto> Conciliaciones, int TotalCount)> ListarAsync(
        int? ano = null,
        int? mes = null,
        string? estado = null,
        int pagina = 1,
        int porPagina = 20);

    /// <summary>
    /// Obtiene detalle completo de una conciliación con todos sus items
    /// </summary>
    Task<ConciliacionBancariaDetalleDto?> ObtenerDetalleAsync(Guid id);

    /// <summary>
    /// Crea una nueva conciliación bancaria
    /// </summary>
    Task<Guid> CrearAsync(ConciliacionBancariaFormDto dto, string usuario);

    /// <summary>
    /// Actualiza una conciliación existente
    /// </summary>
    Task ActualizarAsync(Guid id, ConciliacionBancariaFormDto dto, string usuario);

    /// <summary>
    /// Elimina una conciliación (solo si está en Pendiente)
    /// </summary>
    Task EliminarAsync(Guid id);

    /// <summary>
    /// Agrega un item de conciliación manualmente
    /// </summary>
    Task<Guid> AgregarItemAsync(Guid conciliacionId, ItemConciliacionFormDto dto);

    /// <summary>
    /// Actualiza un item de conciliación
    /// </summary>
    Task ActualizarItemAsync(Guid itemId, ItemConciliacionFormDto dto);

    /// <summary>
    /// Elimina un item de conciliación
    /// </summary>
    Task EliminarItemAsync(Guid itemId);

    /// <summary>
    /// Marca/desmarca un item como conciliado
    /// </summary>
    Task MarcarItemConciliadoAsync(Guid itemId, bool conciliado);

    /// <summary>
    /// Calcula automáticamente los saldos basado en movimientos contables
    /// </summary>
    Task<(decimal SaldoLibros, decimal SaldoBanco, decimal Diferencia)> CalcularSaldosAsync(int ano, int mes);

    /// <summary>
    /// Intenta matching automático entre movimientos bancarios y contables
    /// </summary>
    Task<int> RealizarMatchingAutomaticoAsync(Guid conciliacionId);

    /// <summary>
    /// Importa extracto bancario y crea items automáticamente
    /// </summary>
    Task<int> ImportarExtractoAsync(ImportarExtractoDto dto);

    /// <summary>
    /// Cambia el estado de una conciliación
    /// </summary>
    Task CambiarEstadoAsync(Guid id, string nuevoEstado, string usuario);

    /// <summary>
    /// Verifica si existe conciliación para período específico
    /// </summary>
    Task<bool> ExisteConciliacionAsync(int ano, int mes, Guid? excluyendoId = null);

    /// <summary>
    /// Obtiene resumen consolidado de conciliaciones por año
    /// </summary>
    Task<List<ResumenConciliacionDto>> ObtenerResumenAnualAsync(int ano);
}
