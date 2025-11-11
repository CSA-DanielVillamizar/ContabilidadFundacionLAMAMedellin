using Server.Models;

namespace Server.Services.Deudores;

/// <summary>
/// Servicio de dominio para cálculo de deudores de mensualidad.
/// </summary>
public interface IDeudoresService
{
    /// <summary>
    /// Calcula los deudores dentro de un rango de meses [desde, hasta].
    /// Usa el primer día del mes de las fechas proporcionadas.
    /// Regla de negocio: excluye miembros con Rango = "Asociado" (no pagan mensualidad).
    /// </summary>
    /// <param name="desde">Fecha inicial (primer día de mes). Si es null, se usa el mes de ingreso del miembro.</param>
    /// <param name="hasta">Fecha final (primer día de mes). Si es null, se usa el mes actual.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de filas de deudores con meses pendientes.</returns>
    Task<List<DeudorRow>> CalcularAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el detalle de deudas y pagos de mensualidad para un miembro específico
    /// dentro de un rango de meses [desde, hasta]. Devuelve meses pagados y meses pendientes
    /// según la asignación secuencial de meses por cantidad de mensualidades en los recibos.
    /// </summary>
    /// <param name="miembroId">Identificador del miembro.</param>
    /// <param name="desde">Fecha inicial (primer día de mes). Si es null, se usa el inicio del año actual o la fecha de ingreso (lo que sea más reciente).</param>
    /// <param name="hasta">Fecha final (primer día de mes). Si es null, se usa el mes actual.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Detalle de deudor con meses pagados y pendientes, o null si no existe el miembro.</returns>
    Task<DeudorDetalle?> ObtenerDetalleAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene el precio base de mensualidad en COP (convertido desde USD si aplica).
    /// </summary>
    /// <param name="fechaReferencia">Fecha de referencia para TRM si el concepto está en USD.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Precio de mensualidad en COP.</returns>
    Task<decimal> ObtenerPrecioMensualidadCopAsync(DateOnly? fechaReferencia = null, CancellationToken ct = default);
}

/// <summary>
/// DTO de salida para cada deudor.
/// </summary>
public class DeudorRow
{
    public Guid MiembroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly? Ingreso { get; set; }
    public List<DateOnly> MesesPendientes { get; set; } = new();
}

/// <summary>
/// DTO de salida para el detalle de un deudor.
/// </summary>
public class DeudorDetalle
{
    public Guid MiembroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly? Ingreso { get; set; }
    /// <summary>
    /// Meses cubiertos por pagos dentro del rango solicitado.
    /// </summary>
    public List<DateOnly> MesesPagados { get; set; } = new();
    /// <summary>
    /// Meses aún pendientes dentro del rango solicitado.
    /// </summary>
    public List<DateOnly> MesesPendientes { get; set; } = new();
}
