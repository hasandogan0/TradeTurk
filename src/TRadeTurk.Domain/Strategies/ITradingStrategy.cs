namespace TRadeTurk.Domain.Strategies;

public interface ITradingStrategy
{
    /// <summary>
    /// Stratejinin uygulanıp uygulanmayacağını belirten mantık.
    /// </summary>
    /// <param name="currentPrice">Mevcut fiyat</param>
    /// <param name="averageCost">Ortalama maliyet (opsiyonel)</param>
    /// <returns>İşlem yapılıp yapılmayacağı kararı</returns>
    bool ShouldExecute(decimal currentPrice, decimal? averageCost = null);
}
