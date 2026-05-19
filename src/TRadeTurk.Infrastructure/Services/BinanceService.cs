using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Infrastructure.Services;

public class BinanceService : IBinancePriceService, IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceService> _logger;
    private readonly string _baseUrl;

    public BinanceService(HttpClient httpClient, IConfiguration configuration, ILogger<BinanceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["BinanceApi:BaseUrl"] ?? "https://api.binance.com/api/v3/";

        // Binance requires a User-Agent header
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TradeTurk-App/1.0");
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Requesting real price from Binance for {Symbol}...", symbol);

            // API Endpoint: ticker/price?symbol=BTCUSDT
            var response = await _httpClient.GetFromJsonAsync<BinancePriceResponse>(
                $"{_baseUrl}ticker/price?symbol={symbol}", cancellationToken);

            if (response != null && decimal.TryParse(response.Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
            {
                return price;
            }

            _logger.LogWarning("Could not parse price for {Symbol} from Binance response.", symbol);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for {Symbol} from Binance.", symbol);
            throw;
        }
    }

    public async Task<MarketTickerDto> GetTickerAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var response = await _httpClient.GetFromJsonAsync<BinanceTickerResponse>(
            $"{_baseUrl}ticker/24hr?symbol={normalizedSymbol}", cancellationToken);

        return response == null ? EmptyTicker(normalizedSymbol) : MapTicker(response);
    }

    public async Task<IReadOnlyCollection<MarketTickerDto>> GetTickersAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default)
    {
        var supported = symbols.Select(s => s.Trim().ToUpperInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var response = await _httpClient.GetFromJsonAsync<List<BinanceTickerResponse>>($"{_baseUrl}ticker/24hr", cancellationToken);

        if (response == null)
        {
            return supported.Select(EmptyTicker).ToArray();
        }

        return response
            .Where(t => supported.Contains(t.Symbol))
            .Select(MapTicker)
            .OrderBy(t => Array.IndexOf(MarketSymbols.Supported.ToArray(), t.Symbol))
            .ToArray();
    }

    private static MarketTickerDto MapTicker(BinanceTickerResponse response)
    {
        return new MarketTickerDto
        {
            Symbol = response.Symbol,
            Price = ParseDecimal(response.LastPrice),
            ChangePercent24h = ParseDecimal(response.PriceChangePercent),
            High24h = ParseDecimal(response.HighPrice),
            Low24h = ParseDecimal(response.LowPrice),
            Volume24h = ParseDecimal(response.Volume),
            RetrievedAtUtc = DateTime.UtcNow
        };
    }

    private static MarketTickerDto EmptyTicker(string symbol) => new() { Symbol = symbol, RetrievedAtUtc = DateTime.UtcNow };

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    private class BinancePriceResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }

    private class BinanceTickerResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string LastPrice { get; set; } = "0";
        public string PriceChangePercent { get; set; } = "0";
        public string HighPrice { get; set; } = "0";
        public string LowPrice { get; set; } = "0";
        public string Volume { get; set; } = "0";
    }
}
