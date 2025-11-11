using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services.Reportes;

/// <summary>
/// Implementación del servicio de verificación y autorreparación de tesorería.
/// </summary>
public class VerificacionTesoreriaService : IVerificacionTesoreriaService
{
    private readonly AppDbContext _db;

    public VerificacionTesoreriaService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<VerificacionResultado> VerificarAsync(int anio, int mes, CancellationToken ct = default)
    {
        var inicio = new DateTime(anio, mes, 1);
        var fin = inicio.AddMonths(1).AddTicks(-1);

        // Saldos
        var ingresosAntes = await _db.Recibos
            .Where(r => r.FechaEmision < inicio && r.Estado == EstadoRecibo.Emitido)
            .SumAsync(r => (decimal?)r.TotalCop, ct) ?? 0m;
        var egresosAntes = await _db.Egresos
            .Where(e => e.Fecha < inicio)
            .SumAsync(e => (decimal?)e.ValorCop, ct) ?? 0m;

        var ingresosMes = await _db.Recibos
            .Where(r => r.FechaEmision >= inicio && r.FechaEmision <= fin && r.Estado == EstadoRecibo.Emitido)
            .SumAsync(r => (decimal?)r.TotalCop, ct) ?? 0m;
        var egresosMes = await _db.Egresos
            .Where(e => e.Fecha >= inicio && e.Fecha <= fin)
            .SumAsync(e => (decimal?)e.ValorCop, ct) ?? 0m;

        var saldoInicial = ingresosAntes - egresosAntes;
        var saldoFinal = saldoInicial + ingresosMes - egresosMes;

        // Verificaciones específicas de la migración Octubre 2025
        var reciboSaldo = await _db.Recibos.FirstOrDefaultAsync(r => r.Serie == "AJUSTE" && r.Ano == 2025 && r.Consecutivo == 1, ct);
        var conceptoSaldo = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "SALDO_INICIAL", ct);
        var saldoItemExiste = false;
        var monedaConsistente = true;
        if (reciboSaldo != null)
        {
            var item = await _db.ReciboItems.FirstOrDefaultAsync(i => i.ReciboId == reciboSaldo.Id, ct);
            saldoItemExiste = item != null;

            // Validar moneda = COP (enum = 1)
            if (item != null && (int)item.MonedaOrigen != (int)Moneda.COP)
                monedaConsistente = false;
        }

        return new VerificacionResultado(anio, mes, saldoInicial, ingresosMes, egresosMes, saldoFinal, saldoItemExiste, monedaConsistente);
    }

    /// <inheritdoc />
    public async Task<bool> RepararSaldoInicialAsync(CancellationToken ct = default)
    {
        var recibo = await _db.Recibos.FirstOrDefaultAsync(r => r.Serie == "AJUSTE" && r.Ano == 2025 && r.Consecutivo == 1, ct);
        var concepto = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "SALDO_INICIAL", ct);
        if (recibo == null || concepto == null)
            return false;

        var item = await _db.ReciboItems.FirstOrDefaultAsync(i => i.ReciboId == recibo.Id, ct);
        var reparo = false;

        if (item == null)
        {
            // Inserta item faltante del saldo inicial
            _db.ReciboItems.Add(new ReciboItem
            {
                ReciboId = recibo.Id,
                ConceptoId = concepto.Id,
                Cantidad = 1,
                MonedaOrigen = Moneda.COP,
                PrecioUnitarioMonedaOrigen = 4_718_042m,
                TrmAplicada = 1.00m,
                SubtotalCop = 4_718_042m,
                Notas = "Saldo efectivo del mes anterior (septiembre 2025)"
            });
            reparo = true;
        }
        else if ((int)item.MonedaOrigen != (int)Moneda.COP)
        {
            // Corrige moneda a COP
            item.MonedaOrigen = Moneda.COP;
            reparo = true;
        }

        if (reparo)
            await _db.SaveChangesAsync(ct);

        return reparo;
    }
}
