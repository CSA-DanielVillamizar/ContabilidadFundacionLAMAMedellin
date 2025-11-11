using Server.DTOs;

namespace Server.Services;

/// <summary>
/// Servicio para la gestión de presupuestos por concepto y período.
/// Calcula la ejecución presupuestal basándose en los movimientos financieros reales.
/// </summary>
public interface IPresupuestosService
{
    /// <summary>
    /// Lista presupuestos con filtros opcionales y cálculo de ejecución
    /// </summary>
    Task<(List<PresupuestoDto> presupuestos, int totalCount)> ListarAsync(
        int? ano = null,
        int? mes = null,
        int? conceptoId = null,
        int pagina = 1,
        int porPagina = 50);

    /// <summary>
    /// Obtiene el detalle de un presupuesto específico con ejecución calculada
    /// </summary>
    Task<PresupuestoDetalleDto?> ObtenerDetalleAsync(Guid id);

    /// <summary>
    /// Obtiene consolidado de presupuestos por año (suma todos los meses)
    /// </summary>
    Task<List<PresupuestoConsolidadoDto>> ObtenerConsolidadoAnualAsync(int ano);

    /// <summary>
    /// Crea un nuevo presupuesto para un período y concepto
    /// </summary>
    Task<PresupuestoDetalleDto> CrearAsync(CrearPresupuestoDto dto, string usuario);

    /// <summary>
    /// Actualiza un presupuesto existente (solo monto y notas)
    /// </summary>
    Task<PresupuestoDetalleDto> ActualizarAsync(Guid id, ActualizarPresupuestoDto dto, string usuario);

    /// <summary>
    /// Elimina un presupuesto (soft delete o físico según lógica de negocio)
    /// </summary>
    Task<bool> EliminarAsync(Guid id);

    /// <summary>
    /// Calcula la ejecución de un presupuesto específico consultando movimientos reales
    /// </summary>
    Task<decimal> CalcularEjecucionAsync(Guid presupuestoId);

    /// <summary>
    /// Copia todos los presupuestos de un período origen a un período destino
    /// Útil para replicar presupuestos entre meses o años
    /// </summary>
    Task<int> CopiarPresupuestosAsync(
        int anoOrigen, 
        int mesOrigen, 
        int anoDestino, 
        int mesDestino, 
        string usuario);

    /// <summary>
    /// Verifica si ya existe un presupuesto para un concepto en un período
    /// </summary>
    Task<bool> ExistePresupuestoAsync(int ano, int mes, int conceptoId, Guid? excluyendoId = null);
}
