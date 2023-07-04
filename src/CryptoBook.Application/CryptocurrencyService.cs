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
}