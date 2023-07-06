using CryptoBook.CoinGecko;

namespace CryptoBook.Application;

public class CryptocurrencyService : ICryptocurrencyService
{
    private readonly ICoinGeckoClient coinGeckoClient;

    public CryptocurrencyService(ICoinGeckoClient coinGeckoClient)
    {
        this.coinGeckoClient = coinGeckoClient;
    }

    public async Task<CurrentPriceView> GetCurrentPrice(GetCurrentPriceQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));
        if (query.Coins is null) throw new ArgumentException(nameof(query.Coins));
        if (query.Currencies is null) throw new ArgumentException(nameof(query.Currencies));

        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = query.Coins,
            Currencies = query.Currencies
        };

        var currentPriceView = new CurrentPriceView();
        try
        {
            var simplePriceResponse = await coinGeckoClient.GetSimplePrice(simplePriceRequest);
            if (simplePriceResponse.HasRequestSucceeded)
            {
                var cryptoPrices = simplePriceResponse.CryptocurrencyPrices;
                var currentPrice = new List<CoinPrice>(cryptoPrices?.Count ?? 0);
                foreach (var id in cryptoPrices!.Keys)
                {
                    var pricePerCurrency = new List<Price>(cryptoPrices[id].Count);
                    pricePerCurrency.AddRange(cryptoPrices[id].Keys.Select(currency => new Price(currency, cryptoPrices[id][currency])));

                    var coinPrice = new CoinPrice { Id = id, Prices = pricePerCurrency };
                    currentPrice.Add(coinPrice);
                }

                currentPriceView.CoinPrices = currentPrice;
            }
            else
            {
                currentPriceView.CoinPrices = Array.Empty<CoinPrice>();
            }
        }
        catch (Exception)
        {
            currentPriceView.CoinPrices = Array.Empty<CoinPrice>();
        }

        return currentPriceView;
    }

    public async Task<HistoricalMarketDataView> GetHistoricalMarketData(GetHistoricalMarketDataQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));
        if (query.Coin is null) throw new ArgumentException(nameof(query.Coin));
        if (query.Currency is null) throw new ArgumentException(nameof(query.Currency));

        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = query.Coin,
            Currency = query.Currency,
            Days = query.Days
        };

        var historicalMarketDataView = new HistoricalMarketDataView() { Coin = query.Coin, Currency = query.Currency };
        try
        {
            var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChart(coinMarketChartRequest);
            if (coinMarketChartResponse.HasRequestSucceeded)
            {
                var historicalMarketData = coinMarketChartResponse.HistoricalMarketData;

                var coinPrices = (from price in historicalMarketData.Prices
                    let offsetTime = DateTimeOffset.FromUnixTimeMilliseconds((long)price[0]).ToString("d")
                    let offsetPrice = price[1]
                    select (offsetTime, offsetPrice)).ToList();
                historicalMarketDataView.Prices = coinPrices;

                var coinMarketCaps = (from marketCap in historicalMarketData.MarketCaps
                    let offsetTime = DateTimeOffset.FromUnixTimeMilliseconds((long)marketCap[0]).ToString("d")
                    let offsetPrice = marketCap[1]
                    select (offsetTime, offsetPrice)).ToList();
                historicalMarketDataView.MarketCaps = coinMarketCaps;

                var coinTotalVolumes = (from totalVolume in historicalMarketData.TotalVolumes
                    let offsetTime = DateTimeOffset.FromUnixTimeMilliseconds((long)totalVolume[0]).ToString("d")
                    let offsetPrice = totalVolume[1]
                    select (offsetTime, offsetPrice)).ToList();
                historicalMarketDataView.TotalVolumes = coinTotalVolumes;
            }
            else
            {
                historicalMarketDataView.Prices = new List<(string, double)>();
                historicalMarketDataView.MarketCaps = new List<(string, double)>();
                historicalMarketDataView.TotalVolumes = new List<(string, double)>();
            }
        }
        catch (Exception)
        {
            historicalMarketDataView.Prices = new List<(string, double)>();
            historicalMarketDataView.MarketCaps = new List<(string, double)>();
            historicalMarketDataView.TotalVolumes = new List<(string, double)>();
        }

        return historicalMarketDataView;
    }
}