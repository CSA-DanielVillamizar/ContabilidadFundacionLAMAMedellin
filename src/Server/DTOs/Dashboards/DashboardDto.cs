namespace Server.DTOs.Dashboards;

/// <summary>
/// DTO para Dashboard principal de Tesorería
/// </summary>
public class DashboardTesoreriaDto
{
    // Período de consulta
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    
    // KPIs principales
    public decimal SaldoActual { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal EgresosMes { get; set; }
    public decimal BalanceMes { get; set; }
    
    // Comparación con mes anterior
    public decimal IngresosMesAnterior { get; set; }
    public decimal EgresosMesAnterior { get; set; }
    public decimal PorcentajeCambioIngresos { get; set; }
    public decimal PorcentajeCambioEgresos { get; set; }
    
    // Acumulado anual
    public decimal IngresosAcumuladosAno { get; set; }
    public decimal EgresosAcumuladosAno { get; set; }
    public decimal BalanceAno { get; set; }
    
    // Presupuesto
    public decimal PresupuestoIngresosAno { get; set; }
    public decimal PresupuestoEgresosAno { get; set; }
    public decimal PorcentajeEjecucionIngresos { get; set; }
    public decimal PorcentajeEjecucionEgresos { get; set; }
    
    // Alertas
    public int RecibosVencidosCount { get; set; }
    public int EgresosPendientesAprobacion { get; set; }
    public int ConceptosExcedidosPresupuesto { get; set; }
    public bool RequiereConciliacionBancaria { get; set; }
    
    // Deudores
    public int TotalDeudores { get; set; }
    public decimal MontoTotalDeudas { get; set; }
    
    // Datos para gráficos
    public List<TendenciaDto> TendenciaIngresos { get; set; } = new();
    public List<TendenciaDto> TendenciaEgresos { get; set; } = new();
    public List<ConceptoTopDto> TopConceptosIngresos { get; set; } = new();
    public List<ConceptoTopDto> TopConceptosEgresos { get; set; } = new();
}

/// <summary>
/// DTO para Dashboard principal de Gerencia de Negocios
/// </summary>
public class DashboardGerenciaDto
{
    // Período de consulta
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    
    // KPIs de Ventas
    public decimal VentasMesCOP { get; set; }
    public decimal VentasMesUSD { get; set; }
    public int TotalVentasMes { get; set; }
    public decimal TicketPromedio { get; set; }
    
    // Comparación con mes anterior
    public decimal VentasMesAnteriorCOP { get; set; }
    public decimal PorcentajeCambioVentas { get; set; }
    
    // Acumulado anual
    public decimal VentasAcumuladasAnoCOP { get; set; }
    public decimal VentasAcumuladasAnoUSD { get; set; }
    public int TotalVentasAno { get; set; }
    
    // KPIs de Compras
    public decimal ComprasMesCOP { get; set; }
    public decimal ComprasMesUSD { get; set; }
    public int TotalComprasMes { get; set; }
    
    // Rentabilidad
    public decimal UtilidadBrutaMes { get; set; }
    public decimal MargenBrutoMes { get; set; }
    public decimal UtilidadAcumuladaAno { get; set; }
    
    // Inventario
    public int TotalProductosActivos { get; set; }
    public decimal ValorInventarioCOP { get; set; }
    public decimal ValorInventarioUSD { get; set; }
    public int ProductosStockBajo { get; set; }
    public int ProductosSinStock { get; set; }
    public decimal RotacionInventario { get; set; }
    
    // Alertas
    public int CotizacionesPendientes { get; set; }
    public int DevolucionesPendientes { get; set; }
    public int ComprasPendientesRecepcion { get; set; }
    
    // Datos para gráficos
    public List<TendenciaDto> TendenciaVentas { get; set; } = new();
    public List<ProductoTopDto> TopProductosVendidos { get; set; } = new();
    public List<ProductoTopDto> ProductosStockCritico { get; set; } = new();
    public List<ClienteTopDto> TopClientes { get; set; } = new();
}

/// <summary>
/// DTO para tendencias en gráficos de línea/barras
/// </summary>
public class TendenciaDto
{
    public string Etiqueta { get; set; } = string.Empty; // "Ene 2025", "Feb 2025", etc.
    public decimal Valor { get; set; }
    public int? Mes { get; set; }
    public int? Ano { get; set; }
}

/// <summary>
/// DTO para top conceptos (ingresos/egresos)
/// </summary>
public class ConceptoTopDto
{
    public Guid ConceptoId { get; set; }
    public string ConceptoNombre { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public int TotalTransacciones { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
}

/// <summary>
/// DTO para top productos
/// </summary>
public class ProductoTopDto
{
    public Guid ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal MontoTotalCOP { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
}

/// <summary>
/// DTO para top clientes
/// </summary>
public class ClienteTopDto
{
    public Guid? ClienteId { get; set; }
    public Guid? MiembroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Miembro, Cliente
    public int TotalCompras { get; set; }
    public decimal MontoTotalCOP { get; set; }
    public decimal TicketPromedio { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
