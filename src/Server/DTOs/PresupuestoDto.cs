namespace Server.DTOs;

/// <summary>
/// DTO para listado de presupuestos con cálculo de ejecución
/// </summary>
public class PresupuestoDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoEjecutado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal PorcentajeEjecucion { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para detalle completo de un presupuesto con auditoría
/// </summary>
public class PresupuestoDetalleDto
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoEjecutado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal PorcentajeEjecucion { get; set; }
    public string? Notas { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO para consolidado anual por concepto
/// </summary>
public class PresupuestoConsolidadoDto
{
    public int ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public int Ano { get; set; }
    public decimal MontoPresupuestadoTotal { get; set; }
    public decimal MontoEjecutadoTotal { get; set; }
    public decimal DiferenciaTotal { get; set; }
    public decimal PorcentajeEjecucionPromedio { get; set; }
    public int CantidadMeses { get; set; }
}

/// <summary>
/// DTO para crear un nuevo presupuesto
/// </summary>
public class CrearPresupuestoDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int ConceptoId { get; set; }
    public decimal MontoPresupuestado { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para actualizar un presupuesto existente
/// </summary>
public class ActualizarPresupuestoDto
{
    public decimal MontoPresupuestado { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para solicitud de copia de presupuestos
/// </summary>
public class CopiarPresupuestosDto
{
    public int AnoOrigen { get; set; }
    public int MesOrigen { get; set; }
    public int AnoDestino { get; set; }
    public int MesDestino { get; set; }
}
