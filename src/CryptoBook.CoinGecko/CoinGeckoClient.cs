using System.Text.Json;

namespace CryptoBook.CoinGecko;

using PriceMatrix = IDictionary<string, IDictionary<string, double>>;

public class CoinGeckoClient : ICoinGeckoClient
{
    public const string ApiRootUrl = "https://api.coingecko.com/api/v3";

    private readonly HttpClient httpClient;

    public CoinGeckoClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<SimplePriceResponse> GetSimplePrice(SimplePriceRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Coins is null) throw new ArgumentException(nameof(request.Coins));
        if (request.Currencies is null) throw new ArgumentException(nameof(request.Currencies));

        const char separator = ',';
        var apiArgIds = string.Join(separator, request.Coins);
        var apiArgCurrencies = string.Join(separator, request.Currencies);
        var simplePriceApiUrl = $"{ApiRootUrl}/simple/price?ids={apiArgIds}&vs_currencies={apiArgCurrencies}";

        SimplePriceResponse response;
        try
        {
            var resultStream = await httpClient.GetStreamAsync(simplePriceApiUrl);
            var priceMatrix = await JsonSerializer.DeserializeAsync<PriceMatrix>(resultStream);
            response = new SimplePriceResponse
            {
                HasRequestSucceeded = true,
                CryptocurrencyPrices = priceMatrix ?? new Dictionary<string, IDictionary<string, double>>()
            };
        }
        catch (Exception)
        {
            response = new SimplePriceResponse() { HasRequestSucceeded = false };
        }

        return response;
    }

    public async Task<CoinMarketChartResponse> GetCoinMarketChart(CoinMarketChartRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Coin is null) throw new ArgumentException(nameof(request.Coin));
        if (request.Currency is null) throw new ArgumentException(nameof(request.Currency));

        var coinMarketChartApiUrl = $"{ApiRootUrl}/coins/{request.Coin}/market_chart?vs_currencies={request.Currency}&days={request.Days}";

        CoinMarketChartResponse response;
        try
        {
            var resultStream = await httpClient.GetStreamAsync(coinMarketChartApiUrl);
            var marketChart = await JsonSerializer.DeserializeAsync<CoinMarketChartResponse.MarketChart>(resultStream);
            response = new CoinMarketChartResponse()
            {
                HasRequestSucceeded = true,
                HistoricalMarketData = marketChart ?? new CoinMarketChartResponse.MarketChart()
                {
                    Prices = new List<double[]>(),
                    MarketCaps = new List<double[]>(),
                    TotalVolumes = new List<double[]>()
                }
            };
        }
        catch (Exception)
        {
            response = new CoinMarketChartResponse() { HasRequestSucceeded = false };
        }

        return response;
    }
}