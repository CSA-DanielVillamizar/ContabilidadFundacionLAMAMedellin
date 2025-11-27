using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services.Exchange;

/// <summary>
/// Obtiene la TRM desde la base de datos; si no existe intenta devolver la última conocida.
/// Endpoint de proveedor configurable (no implementado en este esqueleto).
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public ExchangeRateService(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public async Task<decimal> GetUsdCopAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var tasa = await _db.TasasCambio
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Fecha == fecha, ct);
        if (tasa is not null) return tasa.UsdCop;
        var last = await _db.TasasCambio
            .AsNoTracking()
            .OrderByDescending(x => x.Fecha)
            .FirstOrDefaultAsync(ct);
        return last?.UsdCop ?? 4000m; // placeholder si no hay datos
    }
}
