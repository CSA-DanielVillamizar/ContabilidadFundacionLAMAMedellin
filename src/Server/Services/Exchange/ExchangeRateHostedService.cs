using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Server.Data;
using Server.Models;

namespace Server.Services.Exchange;

public class ExchangeRateHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ExchangeRateHostedService> _log;
    private readonly IConfiguration _cfg;

    public ExchangeRateHostedService(IServiceProvider sp, ILogger<ExchangeRateHostedService> log, IConfiguration cfg)
    {
        _sp = sp;
        _log = log;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Esperar un poco antes de la primera ejecución para no bloquear el startup
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // El servidor se está cerrando antes de que iniciemos
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateTodayAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Cancelación normal al apagar el servidor
                _log.LogInformation("Servicio de actualización de TRM cancelado");
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Error actualizando TRM");
            }
            
            // Ejecutar cada 24h
            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Cancelación normal al apagar el servidor
                break;
            }
        }
    }

    private async Task UpdateTodayAsync(CancellationToken ct)
    {
        var providerUrl = _cfg["ExchangeRate:ProviderUrl"] ?? "https://api.exchangerate.host/latest?base=USD&symbols=COP";
        using var client = new HttpClient();
        var resp = await client.GetFromJsonAsync<JsonElement>(providerUrl, ct);
        if (resp.ValueKind == JsonValueKind.Object && resp.TryGetProperty("rates", out var rates) && rates.TryGetProperty("COP", out var cop))
        {
            var val = cop.GetDecimal();
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (!db.TasasCambio.Any(x => x.Fecha == today))
            {
                db.TasasCambio.Add(new TasaCambio { Fecha = today, UsdCop = val, Fuente = providerUrl, ObtenidaAutomaticamente = true });
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
