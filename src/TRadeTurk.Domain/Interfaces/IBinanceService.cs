namespace TRadeTurk.Domain.Interfaces;

public interface IBinanceService
{
    /// <summary>
    /// Binance REST API üzerinden istenen kripto paranın güncel fiyatını çeker.
    /// </summary>
    /// <param name="symbol">Örn: "BTCUSDT"</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Fiyat</returns>
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default);
}
