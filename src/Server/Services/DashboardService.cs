using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Deudores;

namespace Server.Services;

/// <summary>
/// Servicio para obtener estadísticas del dashboard
/// </summary>
public class DashboardService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDeudoresService _deudoresService;

    /// <summary>
    /// Crea el servicio usando un IDbContextFactory para evitar problemas de concurrencia
    /// con DbContext en Blazor Server (múltiples operaciones en paralelo por circuito).
    /// </summary>
    public DashboardService(IDbContextFactory<AppDbContext> dbFactory, IDeudoresService deudoresService)
    {
        _dbFactory = dbFactory;
        _deudoresService = deudoresService;
    }

    /// <summary>
    /// Obtiene las estadísticas generales del dashboard
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfNextMonth = startOfMonth.AddMonths(1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        // Total miembros activos
        var totalMiembros = await db.Miembros
            .Where(m => m.Estado == EstadoMiembro.Activo)
            .CountAsync();

        var totalMiembrosLastMonth = await db.Miembros
            .Where(m => m.Estado == EstadoMiembro.Activo && m.CreatedAt < startOfMonth)
            .CountAsync();

        // Recibos del mes (excluyendo serie "SI" - Saldo Inicial)
        var recibosDelMes = await db.Recibos
            .Where(r => r.FechaEmision >= startOfMonth && r.FechaEmision < startOfNextMonth && r.Serie != "SI")
            .SumAsync(r => (decimal?)r.TotalCop) ?? 0;

        var recibosLastMonth = await db.Recibos
            .Where(r => r.FechaEmision >= startOfLastMonth && r.FechaEmision < startOfMonth && r.Serie != "SI")
            .SumAsync(r => (decimal?)r.TotalCop) ?? 0;

        // Egresos del mes
        var egresosDelMes = await db.Egresos
            .Where(e => e.Fecha >= startOfMonth && e.Fecha < startOfNextMonth)
            .SumAsync(e => (decimal?)e.ValorCop) ?? 0;

        var egresosLastMonth = await db.Egresos
            .Where(e => e.Fecha >= startOfLastMonth && e.Fecha < startOfMonth)
            .SumAsync(e => (decimal?)e.ValorCop) ?? 0;

        // Balance
        var balance = recibosDelMes - egresosDelMes;
        var balanceLastMonth = recibosLastMonth - egresosLastMonth;

        return new DashboardStats
        {
            TotalMiembros = totalMiembros,
            MiembrosCambio = CalcularCambioPercentual(totalMiembrosLastMonth, totalMiembros),
            RecibosDelMes = recibosDelMes,
            RecibosCambio = CalcularCambioPercentual(recibosLastMonth, recibosDelMes),
            EgresosDelMes = egresosDelMes,
            EgresosCambio = CalcularCambioPercentual(egresosLastMonth, egresosDelMes),
            Balance = balance,
            BalanceCambio = CalcularCambioPercentual(balanceLastMonth, balance)
        };
    }

    /// <summary>
    /// Serie mensual de ingresos, egresos y balance para los últimos 'meses' hasta 'hasta' (UTC, al inicio del mes).
    /// </summary>
    public async Task<DashboardSeries> GetSeriesMensualesAsync(int meses = 12, DateTime? hasta = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var now = hasta?.ToUniversalTime() ?? DateTime.UtcNow;
        var endMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startMonth = endMonth.AddMonths(-(meses - 1));
        var endExclusive = endMonth.AddMonths(1);

        // Prepara mapa de meses
        var labels = new List<string>();
        var ingresos = new List<decimal>();
        var egresos = new List<decimal>();
        var balance = new List<decimal>();

        // Trae agregados por mes en un solo query por tabla (excluyendo serie "SI" - Saldo Inicial)
        var ingresosAgg = await db.Recibos
            .Where(r => r.FechaEmision >= startMonth && r.FechaEmision < endExclusive && r.Estado == EstadoRecibo.Emitido && r.Serie != "SI")
            .GroupBy(r => new { r.FechaEmision.Year, r.FechaEmision.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalCop) })
            .ToListAsync();

        var egresosAgg = await db.Egresos
            .Where(e => e.Fecha >= startMonth && e.Fecha < endExclusive)
            .GroupBy(e => new { e.Fecha.Year, e.Fecha.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.ValorCop) })
            .ToListAsync();

        // Ensambla las series en orden cronológico
        for (var dt = startMonth; dt < endExclusive; dt = dt.AddMonths(1))
        {
            var y = dt.Year; var m = dt.Month;
            var ingresoMes = ingresosAgg.FirstOrDefault(x => x.Year == y && x.Month == m)?.Total ?? 0m;
            var egresoMes = egresosAgg.FirstOrDefault(x => x.Year == y && x.Month == m)?.Total ?? 0m;
            labels.Add(new DateTime(y, m, 1).ToString("MMM yyyy", new System.Globalization.CultureInfo("es-CO")));
            ingresos.Add(ingresoMes);
            egresos.Add(egresoMes);
            balance.Add(ingresoMes - egresoMes);
        }

        return new DashboardSeries
        {
            Labels = labels,
            Ingresos = ingresos,
            Egresos = egresos,
            Balance = balance
        };
    }

    /// <summary>
    /// Top conceptos por volumen total (COP) en el rango de fechas dado. Retorna hasta 'top' conceptos ordenados descendentemente.
    /// </summary>
    public async Task<List<ConceptoTop>> GetTopConceptosAsync(int top = 5, DateTime? desde = null, DateTime? hasta = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var inicio = desde?.ToUniversalTime() ?? new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin = hasta?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(1);

        var items = await db.ReciboItems
            .Include(ri => ri.Recibo)
            .Include(ri => ri.Concepto)
            .Where(ri => ri.Recibo.FechaEmision >= inicio && ri.Recibo.FechaEmision < fin && ri.Recibo.Estado == EstadoRecibo.Emitido && ri.Recibo.Serie != "SI")
            .GroupBy(ri => new { ri.Concepto.Nombre, ri.Concepto.Codigo })
            .Select(g => new { g.Key.Nombre, g.Key.Codigo, Total = g.Sum(x => x.SubtotalCop) })
            .OrderByDescending(x => x.Total)
            .Take(top)
            .ToListAsync();

        return items.Select(x => new ConceptoTop { Nombre = x.Nombre, Codigo = x.Codigo, Total = x.Total }).ToList();
    }

    /// <summary>
    /// Obtiene los últimos recibos
    /// </summary>
    public async Task<List<ReciboResumen>> GetUltimosRecibosAsync(int cantidad = 5)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.Recibos
            .Include(r => r.Miembro)
            .OrderByDescending(r => r.FechaEmision)
            .Take(cantidad)
            .Select(r => new ReciboResumen
            {
                Id = r.Id,
                Numero = r.Serie + "-" + r.Ano + "-" + r.Consecutivo.ToString("D6"),
                Fecha = r.FechaEmision,
                MiembroNombre = r.Miembro != null ? r.Miembro.NombreCompleto : "N/A",
                TotalCop = r.TotalCop,
                Estado = r.Estado.ToString()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los top deudores
    /// </summary>
    public async Task<List<DeudorResumen>> GetTopDeudoresAsync(int cantidad = 5)
    {
        // Obtener listado completo de deudores usando DeudoresService
        var deudores = await _deudoresService.CalcularAsync();

        // Ordenar por mayor número de meses pendientes y tomar los primeros N
        var topDeudores = deudores
            .OrderByDescending(d => d.MesesPendientes.Count)
            .Take(cantidad)
            .Select(d => new DeudorResumen
            {
                MiembroId = d.MiembroId,
                NombreCompleto = d.Nombre,
                NumeroSocio = 0, // No está disponible en DeudorRow
                DeudaTotal = 0, // No está disponible en DeudorRow, requiere ConceptoService para calcular
                MesesPendientes = d.MesesPendientes.Count
            })
            .ToList();

        return topDeudores;
    }

    private string CalcularCambioPercentual(decimal anterior, decimal actual)
    {
        if (anterior == 0) return actual > 0 ? "+100%" : "0%";
        
        var cambio = ((actual - anterior) / anterior) * 100;
        var signo = cambio >= 0 ? "+" : "";
        return $"{signo}{cambio:F1}%";
    }
}

/// <summary>
/// Estadísticas del dashboard
/// </summary>
public class DashboardStats
{
    public int TotalMiembros { get; set; }
    public string MiembrosCambio { get; set; } = string.Empty;
    public decimal RecibosDelMes { get; set; }
    public string RecibosCambio { get; set; } = string.Empty;
    public decimal EgresosDelMes { get; set; }
    public string EgresosCambio { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BalanceCambio { get; set; } = string.Empty;
}

/// <summary>
/// Series mensuales para gráficos del dashboard
/// </summary>
public class DashboardSeries
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Ingresos { get; set; } = new();
    public List<decimal> Egresos { get; set; } = new();
    public List<decimal> Balance { get; set; } = new();
}

public class ReciboResumen
{
    public Guid Id { get; set; }
    /// <summary>
    /// Número compuesto del recibo (Serie-Año-Consecutivo con padding), ej: LM-2025-000123
    /// </summary>
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string MiembroNombre { get; set; } = string.Empty;
    public decimal TotalCop { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class DeudorResumen
{
    public Guid MiembroId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int NumeroSocio { get; set; }
    public decimal DeudaTotal { get; set; }
    public int MesesPendientes { get; set; }
}

public class ConceptoTop
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
