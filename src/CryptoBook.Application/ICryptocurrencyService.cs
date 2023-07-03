namespace CryptoBook.Application;

public interface ICryptocurrencyService
{
    Task<CurrentPriceView> GetCurrentPrice(GetCurrentPriceQuery query);
}