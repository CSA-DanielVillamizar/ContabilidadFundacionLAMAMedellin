using System.ComponentModel.DataAnnotations;

namespace Server.Models;

/// <summary>
/// Monedas soportadas.
/// </summary>
public enum Moneda { COP = 1, USD = 2 }

/// <summary>
/// Periodicidad de un concepto.
/// </summary>
public enum Periodicidad { Unico = 0, Mensual = 1, Anual = 2 }

/// <summary>
/// Concepto de cobro (mensualidad, parches, etc.).
/// </summary>
public class Concepto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty; // ej: MENSUALIDAD
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Moneda Moneda { get; set; } = Moneda.COP;
    public decimal PrecioBase { get; set; }
    public bool EsRecurrente { get; set; }
    public Periodicidad Periodicidad { get; set; }
    public bool EsIngreso { get; set; } = true;
}

/// <summary>
/// Tasa de cambio USD -> COP por fecha.
/// </summary>
public class TasaCambio
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal UsdCop { get; set; }
    public string Fuente { get; set; } = "Manual";
    public bool ObtenidaAutomaticamente { get; set; } = false;
    public bool EsOficial { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Estado del recibo.
/// </summary>
public enum EstadoRecibo { Borrador = 0, Emitido = 1, Anulado = 2 }

/// <summary>
/// Recibo principal con items y pago opcional.
/// </summary>
public class Recibo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Serie { get; set; } = "LM";
    public int Ano { get; set; } = DateTime.UtcNow.Year;
    public int Consecutivo { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public EstadoRecibo Estado { get; set; } = EstadoRecibo.Borrador;

    public Guid? MiembroId { get; set; }
    public Miembro? Miembro { get; set; }
    public string? TerceroLibre { get; set; }

    public decimal TotalCop { get; set; }
    public string? Observaciones { get; set; }

    public List<ReciboItem> Items { get; set; } = new();
    public Pago? Pago { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";

    /// <summary>
    /// Hash determinístico para deduplicación de imports (SHA256 de FechaEmision|TotalCop|Serie|Consecutivo|MiembroId).
    /// Permite idempotencia: el import puede ejecutarse múltiples veces sin crear duplicados.
    /// </summary>
    [MaxLength(64)]
    public string? ImportRowHash { get; set; }
}

/// <summary>
/// Item dentro de un recibo.
/// </summary>
public class ReciboItem
{
    public int Id { get; set; }
    public Guid ReciboId { get; set; }
    public Recibo Recibo { get; set; } = null!;

    public int ConceptoId { get; set; }
    public Concepto Concepto { get; set; } = null!;

    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitarioMonedaOrigen { get; set; }
    public Moneda MonedaOrigen { get; set; } = Moneda.COP;
    public decimal? TrmAplicada { get; set; }
    public decimal SubtotalCop { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// Pago asociado a un recibo.
/// </summary>
public enum MetodoPago { Efectivo, Transferencia, Nequi, Daviplata, Tarjeta }

public class Pago
{
    public int Id { get; set; }
    public Guid ReciboId { get; set; }
    public Recibo Recibo { get; set; } = null!;

    public MetodoPago Metodo { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public decimal ValorPagadoCop { get; set; }
    public string UsuarioRegistro { get; set; } = "system";
}
