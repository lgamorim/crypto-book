using System.Text.Json.Serialization;

namespace CryptoBook.CoinGecko;

public class CoinMarketChartResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; set; }
    public MarketChart HistoricalMarketData { get; set; }

    public class MarketChart
    {
        [JsonPropertyName("prices")] public IList<double[]> Prices { get; set; }

        [JsonPropertyName("market_caps")] public IList<double[]> MarketCaps { get; set; }

        [JsonPropertyName("total_volumes")] public IList<double[]> TotalVolumes { get; set; }
    }
}