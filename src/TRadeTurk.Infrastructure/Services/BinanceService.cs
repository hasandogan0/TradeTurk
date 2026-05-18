using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public class BinanceService : IBinancePriceService
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

    private class BinancePriceResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
