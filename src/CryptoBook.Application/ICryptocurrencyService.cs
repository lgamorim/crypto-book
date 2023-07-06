namespace CryptoBook.Application;

public interface ICryptocurrencyService
{
    Task<CurrentPriceView> GetCurrentPrice(GetCurrentPriceQuery query);
    Task<HistoricalMarketDataView> GetHistoricalMarketData(GetHistoricalMarketDataQuery query);
}