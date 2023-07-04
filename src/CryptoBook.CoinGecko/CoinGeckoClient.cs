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
}