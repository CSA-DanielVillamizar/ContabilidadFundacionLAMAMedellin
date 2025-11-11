namespace Server.DTOs.Presupuestos;

/// <summary>
/// DTO para visualización de presupuestos en listados
/// </summary>
public class PresupuestoDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty; // "Enero 2025"
    public Guid ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public string ConceptoTipo { get; set; } = string.Empty; // Ingreso, Egreso
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoEjecutado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal PorcentajeEjecucion { get; set; }
    public string Estado { get; set; } = string.Empty; // Normal, Alerta, Excedido
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear/editar presupuestos
/// </summary>
public class PresupuestoFormDto
{
    public Guid? Id { get; set; }
    public int Ano { get; set; } = DateTime.Now.Year;
    public int Mes { get; set; } = DateTime.Now.Month;
    public Guid ConceptoId { get; set; }
    public decimal MontoPresupuestado { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para detalle completo del presupuesto
/// </summary>
public class PresupuestoDetalleDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    
    // Concepto
    public Guid ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public string ConceptoTipo { get; set; } = string.Empty;
    
    // Montos
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoEjecutado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal PorcentajeEjecucion { get; set; }
    
    public string? Notas { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO para reporte consolidado de presupuesto
/// </summary>
public class PresupuestoConsolidadoDto
{
    public int Ano { get; set; }
    public int? Mes { get; set; } // null = anual
    public string Periodo { get; set; } = string.Empty;
    
    public decimal TotalPresupuestadoIngresos { get; set; }
    public decimal TotalEjecutadoIngresos { get; set; }
    public decimal TotalPresupuestadoEgresos { get; set; }
    public decimal TotalEjecutadoEgresos { get; set; }
    
    public decimal BalancePresupuestado { get; set; }
    public decimal BalanceEjecutado { get; set; }
    
    public List<PresupuestoDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para copiar presupuesto de un período a otro
/// </summary>
public class PresupuestoCopiarDto
{
    public int AnoOrigen { get; set; }
    public int MesOrigen { get; set; }
    public int AnoDestino { get; set; }
    public int MesDestino { get; set; }
    public bool SobreescribirExistentes { get; set; }
    public decimal? FactorAjuste { get; set; } // null = copiar igual, 1.10 = aumentar 10%
}
