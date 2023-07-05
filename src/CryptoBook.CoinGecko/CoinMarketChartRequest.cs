namespace CryptoBook.CoinGecko;

public class CoinMarketChartRequest : IApiRequest
{
    public string Coin { get; set; }
    public string Currency { get; set; }
    public int Days { get; set; }
}