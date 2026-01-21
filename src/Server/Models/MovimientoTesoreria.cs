namespace Server.Models;

/// <summary>
/// Movimiento de tesorería: registra ingresos/egresos reales sobre una cuenta financiera.
/// Solo los aprobados impactan el saldo.
/// </summary>
public enum TipoMovimientoTesoreria { Ingreso = 1, Egreso = 2 }
public enum MedioPagoTesoreria { Transferencia = 1, Consignacion = 2, Efectivo = 3, Cheque = 4, Nequi = 5, Daviplata = 6, Tarjeta = 7 }
public enum EstadoMovimientoTesoreria { Borrador = 0, Aprobado = 1, Anulado = 2 }

public class MovimientoTesoreria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeroMovimiento { get; set; } = string.Empty; // Consecutivo: MV-YYYY-#####
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public TipoMovimientoTesoreria Tipo { get; set; } = TipoMovimientoTesoreria.Ingreso;

    // Cuenta afectada
    public Guid CuentaFinancieraId { get; set; }
    public CuentaFinanciera? CuentaFinanciera { get; set; }

    // Clasificación
    public Guid? FuenteIngresoId { get; set; }
    public FuenteIngreso? FuenteIngreso { get; set; }

    public Guid? CategoriaEgresoId { get; set; }
    public CategoriaEgreso? CategoriaEgreso { get; set; }

    // Datos del movimiento
    public decimal Valor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public MedioPagoTesoreria Medio { get; set; } = MedioPagoTesoreria.Transferencia;
    public string? ReferenciaTransaccion { get; set; }

    // Tercero (opcional)
    public Guid? TerceroId { get; set; }
    public string? TerceroNombre { get; set; }

    // Soporte
    public string? SoporteUrl { get; set; }

    // Estado
    public EstadoMovimientoTesoreria Estado { get; set; } = EstadoMovimientoTesoreria.Borrador;
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioAprobacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? UsuarioAnulacion { get; set; }

    // Relación con Recibo (opcional)
    public Guid? ReciboId { get; set; }
    public Recibo? Recibo { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "seed";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Trazabilidad de importación (para carga histórica desde Excel)
    public string? ImportHash { get; set; }
    public string? ImportSource { get; set; }
    public string? ImportSheet { get; set; }
    public int? ImportRowNumber { get; set; }
    public DateTime? ImportedAtUtc { get; set; }
    public bool ImportHasBalanceMismatch { get; set; }
    public decimal? ImportBalanceExpected { get; set; }
    public decimal? ImportBalanceFound { get; set; }
}
