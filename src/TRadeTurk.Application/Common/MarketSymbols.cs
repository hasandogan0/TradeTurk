namespace TRadeTurk.Application.Common;

public static class MarketSymbols
{
    public static readonly IReadOnlyCollection<string> Supported = new[]
    {
        "BTCUSDT",
        "ETHUSDT",
        "BNBUSDT",
        "SOLUSDT",
        "XRPUSDT",
        "ADAUSDT",
        "DOGEUSDT",
        "AVAXUSDT",
        "DOTUSDT",
        "LINKUSDT",
        "MATICUSDT",
        "LTCUSDT",
        "TRXUSDT",
        "ATOMUSDT",
        "NEARUSDT"
    };

    public static bool IsSupported(string symbol)
    {
        return Supported.Contains(symbol.Trim().ToUpperInvariant());
    }
}
