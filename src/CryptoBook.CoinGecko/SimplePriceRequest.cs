namespace CryptoBook.CoinGecko;

public class SimplePriceRequest : IApiRequest
{
    public IEnumerable<string> Coins { get; set; }
    public IEnumerable<string> Currencies { get; set; }
}