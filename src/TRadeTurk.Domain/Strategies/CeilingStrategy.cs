namespace TRadeTurk.Domain.Strategies;

/// <summary>
/// Tavan (Ceiling) Stratejisi: Fiyat belirlenen tavan (direnç veya take-profit) seviyesini aşarsa veya ulaşırsa işlem sinyali üretir.
/// </summary>
public class CeilingStrategy : ITradingStrategy
{
    private readonly decimal _ceilingPrice;

    public CeilingStrategy(decimal ceilingPrice)
    {
        _ceilingPrice = ceilingPrice;
    }

    public bool ShouldExecute(decimal currentPrice, decimal? averageCost = null)
    {
        // Fiyat belirlenen tavan seviyesine ulaştıysa veya üstündeyse tetiklenir
        return currentPrice >= _ceilingPrice;
    }
}
