namespace Server.Services.Exchange;

public interface IExchangeRateService
{
    Task<decimal> GetUsdCopAsync(DateOnly fecha, CancellationToken ct = default);
}
