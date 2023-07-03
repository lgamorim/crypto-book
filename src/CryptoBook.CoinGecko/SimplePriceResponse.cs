namespace CryptoBook.CoinGecko;

using PriceMatrix = IDictionary<string, IDictionary<string, double>>;

public class SimplePriceResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; set; }
    public PriceMatrix? CryptocurrencyPrices { get; set; }
}