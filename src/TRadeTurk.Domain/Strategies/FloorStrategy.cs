namespace TRadeTurk.Domain.Strategies;

/// <summary>
/// Taban (Floor) Stratejisi: Fiyat belirlenen taban seviyesine (destek veya stop-loss) düşerse işlem sinyali üretir.
/// </summary>
public class FloorStrategy : ITradingStrategy
{
    private readonly decimal _floorPrice;

    public FloorStrategy(decimal floorPrice)
    {
        _floorPrice = floorPrice;
    }

    public bool ShouldExecute(decimal currentPrice, decimal? averageCost = null)
    {
        // Fiyat belirlenen taban seviyesine düştüyse veya altındaysa tetiklenir
        return currentPrice <= _floorPrice;
    }
}
