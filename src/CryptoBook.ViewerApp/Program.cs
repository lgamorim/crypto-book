using CryptoBook.Application;
using CryptoBook.CoinGecko;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = ConfigureServices();

try
{
    const char separator = ',';
    var coins = args[0].Split(separator).Select(str => str.Trim()).ToArray();
    var currencies = args[1].Split(separator).Select(str => str.Trim()).ToArray();

    var cryptocurrencyService = serviceProvider.GetService<ICryptocurrencyService>();
    if (cryptocurrencyService != null)
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery() { Coins = coins, Currencies = currencies };
        var currentPrice = await cryptocurrencyService.GetCurrentPrice(getCurrentPriceQuery);
        foreach (var coinPrice in currentPrice.CoinPrices)
        {
            Console.WriteLine(coinPrice);
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
}

ServiceProvider ConfigureServices()
{
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddSingleton<ICryptocurrencyService, CryptocurrencyService>();
    serviceCollection.AddSingleton<ICoinGeckoClient, CoinGeckoClient>();
    serviceCollection.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>();

    return serviceCollection.BuildServiceProvider();
}