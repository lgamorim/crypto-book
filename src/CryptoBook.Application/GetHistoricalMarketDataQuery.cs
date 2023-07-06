namespace CryptoBook.Application;

public class GetHistoricalMarketDataQuery
{
    public string Coin { get; set; }
    public string Currency { get; set; }
    public int Days { get; set; }
}