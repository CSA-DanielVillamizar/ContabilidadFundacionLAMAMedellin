using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Audit;

namespace Server.Services.CierreContable;

/// <summary>
/// Servicio para gestión de cierres contables mensuales.
/// Permite cerrar períodos y validar si un mes está cerrado.
/// </summary>
public class CierreContableService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAuditService _auditService;

    public CierreContableService(IDbContextFactory<AppDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    /// <summary>
    /// Verifica si un mes está cerrado
    /// </summary>
    public async Task<bool> EsMesCerradoAsync(int ano, int mes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CierresMensuales
            .AnyAsync(c => c.Ano == ano && c.Mes == mes);
    }

    /// <summary>
    /// Verifica si una fecha pertenece a un mes cerrado
    /// </summary>
    public async Task<bool> EsFechaCerradaAsync(DateTime fecha)
    {
        return await EsMesCerradoAsync(fecha.Year, fecha.Month);
    }

    /// <summary>
    /// Obtiene todos los cierres mensuales registrados
    /// </summary>
    public async Task<List<CierreMensual>> ObtenerCierresAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CierresMensuales
            .OrderByDescending(c => c.Ano)
            .ThenByDescending(c => c.Mes)
            .ToListAsync();
    }

    /// <summary>
    /// Cierra un mes contable. Calcula totales y registra el cierre.
    /// </summary>
    /// <param name="ano">Año a cerrar</param>
    /// <param name="mes">Mes a cerrar (1-12)</param>
    /// <param name="usuario">Usuario que realiza el cierre</param>
    /// <param name="observaciones">Notas opcionales del cierre</param>
    /// <returns>El registro de cierre creado</returns>
    /// <exception cref="InvalidOperationException">Si el mes ya está cerrado</exception>
    public async Task<CierreMensual> CerrarMesAsync(int ano, int mes, string usuario, string? observaciones = null)
    {
        if (mes < 1 || mes > 12)
            throw new ArgumentException("El mes debe estar entre 1 y 12", nameof(mes));

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Validar que no esté ya cerrado
        var existeCierre = await context.CierresMensuales
            .AnyAsync(c => c.Ano == ano && c.Mes == mes);

        if (existeCierre)
            throw new InvalidOperationException($"El mes {mes}/{ano} ya está cerrado.");

        // Calcular fechas del período
        var primerDia = new DateTime(ano, mes, 1);
        var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

        // Calcular saldo inicial: buscar recibo serie "SI" del mes actual, o calcular acumulado histórico
        decimal saldoInicial;
        var reciboSaldoInicial = await context.Recibos
            .FirstOrDefaultAsync(r => r.Serie == "SI" && r.Ano == ano && r.Consecutivo == mes);

        if (reciboSaldoInicial != null)
        {
            // Usar el saldo inicial registrado
            saldoInicial = reciboSaldoInicial.TotalCop;
        }
        else
        {
            // Calcular saldo inicial desde el histórico (ingresos - egresos hasta fin del mes anterior)
            var finMesAnterior = primerDia.AddDays(-1);
            
            var ingresosAnteriores = await context.Recibos
                .Where(r => r.FechaEmision <= finMesAnterior && r.Estado == EstadoRecibo.Emitido && r.Serie != "SI")
                .SumAsync(r => (decimal?)r.TotalCop) ?? 0m;

            var egresosAnteriores = await context.Egresos
                .Where(e => e.Fecha <= finMesAnterior)
                .SumAsync(e => (decimal?)e.ValorCop) ?? 0m;

            saldoInicial = ingresosAnteriores - egresosAnteriores;
        }

        // Calcular totales del mes a cerrar (excluyendo serie "SI" - Saldo Inicial)
        var ingresosMes = await context.Recibos
            .Where(r => r.FechaEmision >= primerDia && r.FechaEmision <= ultimoDia && r.Estado == EstadoRecibo.Emitido && r.Serie != "SI")
            .SumAsync(r => (decimal?)r.TotalCop) ?? 0m;

        var egresosMes = await context.Egresos
            .Where(e => e.Fecha >= primerDia && e.Fecha <= ultimoDia)
            .SumAsync(e => (decimal?)e.ValorCop) ?? 0m;

        var saldoFinal = saldoInicial + ingresosMes - egresosMes;

        // Crear registro de cierre
        var cierre = new CierreMensual
        {
            Id = Guid.NewGuid(),
            Ano = ano,
            Mes = mes,
            FechaCierre = DateTime.Now,
            UsuarioCierre = usuario,
            Observaciones = observaciones,
            SaldoInicialCalculado = saldoInicial,
            TotalIngresos = ingresosMes,
            TotalEgresos = egresosMes,
            SaldoFinal = saldoFinal,
            CreatedAt = DateTime.Now,
            CreatedBy = usuario
        };

        context.CierresMensuales.Add(cierre);
        await context.SaveChangesAsync();

        // ✅ AUDITORÍA: Registrar cierre contable mensual
        await _auditService.LogAsync(
            entityType: "CierreMensual",
            entityId: cierre.Id.ToString(),
            action: "CIERRE_MENSUAL_EJECUTADO",
            userName: usuario,
            newValues: new
            {
                Periodo = $"{mes:D2}/{ano}",
                SaldoInicial = saldoInicial,
                TotalIngresos = ingresosMes,
                TotalEgresos = egresosMes,
                SaldoFinal = saldoFinal,
                Observaciones = observaciones ?? "Sin observaciones"
            },
            additionalInfo: $"Cierre contable ejecutado para {mes:D2}/{ano} - Saldo Final: ${saldoFinal:N0} COP"
        );

        return cierre;
    }

    /// <summary>
    /// Obtiene el último mes cerrado
    /// </summary>
    public async Task<CierreMensual?> ObtenerUltimoCierreAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CierresMensuales
            .OrderByDescending(c => c.Ano)
            .ThenByDescending(c => c.Mes)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Reabre un mes cerrado (solo para Admin).
    /// Registra auditoría obligatoria del motivo de reapertura.
    /// </summary>
    /// <param name="ano">Año del período a reabrir</param>
    /// <param name="mes">Mes del período a reabrir</param>
    /// <param name="motivo">Motivo de reapertura (obligatorio para auditoría)</param>
    /// <param name="usuarioAdmin">Usuario Admin que autoriza la reapertura</param>
    /// <returns>El registro de cierre que se reabrió (con FechaCierre anulada en auditoría)</returns>
    public async Task<CierreMensual?> ReabrirMesAsync(int ano, int mes, string motivo, string usuarioAdmin)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Debe proporcionar un motivo de reapertura para auditoría", nameof(motivo));

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cierre = await context.CierresMensuales
            .FirstOrDefaultAsync(c => c.Ano == ano && c.Mes == mes);

        if (cierre is null)
            throw new InvalidOperationException($"No existe cierre registrado para {mes:D2}/{ano}");

        // ✅ AUDITORÍA OBLIGATORIA: Registrar antes de eliminar
        await _auditService.LogAsync(
            entityType: "CierreMensual",
            entityId: cierre.Id.ToString(),
            action: "CIERRE_REABIERTO",
            userName: usuarioAdmin,
            oldValues: new
            {
                Ano = cierre.Ano,
                Mes = cierre.Mes,
                FechaCierre = cierre.FechaCierre.ToString("yyyy-MM-dd HH:mm:ss"),
                UsuarioCierre = cierre.UsuarioCierre,
                SaldoFinal = cierre.SaldoFinal
            },
            newValues: new
            {
                Estado = "REABIERTO",
                MotivoReapertura = motivo
            },
            additionalInfo: $"REAPERTURA CRÍTICA: El período {mes:D2}/{ano} fue reabierto por {usuarioAdmin}. Motivo: {motivo}"
        );

        // Eliminar el cierre para permitir que el período sea editable nuevamente
        context.CierresMensuales.Remove(cierre);
        await context.SaveChangesAsync();

        return cierre;
    }
}
