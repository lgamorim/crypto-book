namespace CryptoBook.Application;

public class HistoricalMarketDataView
{
    public string Coin { get; set; }
    public string Currency { get; set; }
    public IList<(string, double)> Prices { get; set; }
    public IList<(string, double)> MarketCaps { get; set; }
    public IList<(string, double)> TotalVolumes { get; set; }
}