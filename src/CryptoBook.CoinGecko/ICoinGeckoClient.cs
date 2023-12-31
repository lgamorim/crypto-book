namespace CryptoBook.CoinGecko;

public interface ICoinGeckoClient
{
    Task<SimplePriceResponse> GetSimplePrice(SimplePriceRequest request);
    Task<CoinMarketChartResponse> GetCoinMarketChart(CoinMarketChartRequest request);
}