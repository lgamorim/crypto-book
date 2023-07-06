using CryptoBook.CoinGecko;
using FluentAssertions;
using Moq;

namespace CryptoBook.Application.UnitTests;

public class CryptocurrencyServiceTests
{
    [Fact]
    public async void Should_CallGetSimplePriceAndReturnViewWithCorrectMapping_When_GetCurrentPriceQueryIsValid()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = getCurrentPriceQuery.Coins,
            Currencies = getCurrentPriceQuery.Currencies
        };
        var simplePriceResponse = new SimplePriceResponse()
        {
            HasRequestSucceeded = true,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, double>>()
            {
                { "bitcoin", new Dictionary<string, double> { { "eur", 28135 }, { "usd", 30628 }, { "gbp", 24166 }, { "jpy", 4429566 } } },
                { "ethereum", new Dictionary<string, double> { { "eur", 1799 }, { "usd", 1958 }, { "gbp", 1545.29 }, { "jpy", 283242 } } },
                { "cardano", new Dictionary<string, double> { { "eur", 0.269991 }, { "usd", 0.293915 }, { "gbp", 0.231909 }, { "jpy", 42.51 } } }
            }
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        coinGeckoClientMoq.Setup(_ => _.GetSimplePrice(It.IsAny<SimplePriceRequest>()))
            .ReturnsAsync((SimplePriceRequest _) => simplePriceResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);
        var currentPriceView = await cryptocurrencyService.GetCurrentPrice(getCurrentPriceQuery);

        Func<SimplePriceRequest, SimplePriceRequest, bool> isExpectedRequest = (request, expected) =>
            Equals(request.Coins, expected.Coins) && Equals(request.Currencies, expected.Currencies);
        coinGeckoClientMoq.Verify(_ => _.GetSimplePrice(It.Is<SimplePriceRequest>(request => isExpectedRequest(request, simplePriceRequest))),
            Times.Once);

        currentPriceView.Should().NotBeNull();
        currentPriceView.CoinPrices.Should().NotBeNull();
        currentPriceView.CoinPrices.Count().Should().Be(3);
        AssertCurrentPriceViewCoinPrices(currentPriceView.CoinPrices);

        void AssertCurrentPriceViewCoinPrices(IEnumerable<CoinPrice> coinPrices)
        {
            var sampledCryptoPrices = simplePriceResponse.CryptocurrencyPrices;
            foreach (var coinPrice in coinPrices)
            {
                sampledCryptoPrices.Should().ContainKey(coinPrice.Id);
                var sampledCoinPrice = sampledCryptoPrices[coinPrice.Id];
                foreach (var price in coinPrice.Prices)
                {
                    sampledCoinPrice.Should().ContainKey(price.Currency);
                    price.Value.Should().Be(sampledCoinPrice[price.Currency]);
                }
            }
        }
    }

    [Fact]
    public async void Should_GetCurrentPriceThrowArgumentNullException_When_GetCurrentPriceQueryIsNull()
    {
        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPrice(null!);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async void Should_GetCurrentPriceThrowArgumentException_When_GetCurrentPriceQueryCurrenciesIsNull()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = null!
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPrice(getCurrentPriceQuery);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Currencies");
    }

    [Fact]
    public async void Should_GetCurrentPriceThrowArgumentException_When_GetCurrentPriceQueryCoinsIsNull()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = null!,
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPrice(getCurrentPriceQuery);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Coins");
    }

    [Fact]
    public async void Should_CallGetHistoricalMarketDataAndReturnViewWithCorrectMapping_When_GetHistoricalMarketDataQueryIsValid()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = getHistoricalMarketDataQuery.Coin,
            Currency = getHistoricalMarketDataQuery.Currency,
            Days = getHistoricalMarketDataQuery.Days
        };
        var coinMarketChartResponse = new CoinMarketChartResponse()
        {
            HasRequestSucceeded = true,
            HistoricalMarketData = new CoinMarketChartResponse.MarketChart()
            {
                Prices = new List<double[]> { new[] { 1688468488622d, 28477.63754439077 }, new[] { 1688554643000d, 28058.665368361602 } },
                MarketCaps = new List<double[]> { new[] { 1688468488622d, 552996577247.077 }, new[] { 1688554643000d, 544504299176.5765 } },
                TotalVolumes = new List<double[]> { new[] { 1688468488622d, 13732072142.597347 }, new[] { 1688554643000d, 9014016349.460764 } }
            }
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        coinGeckoClientMoq.Setup(_ => _.GetCoinMarketChart(It.IsAny<CoinMarketChartRequest>()))
            .ReturnsAsync((CoinMarketChartRequest _) => coinMarketChartResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);
        var historicalMarketDataView = await cryptocurrencyService.GetHistoricalMarketData(getHistoricalMarketDataQuery);

        Func<CoinMarketChartRequest, CoinMarketChartRequest, bool> isExpectedRequest = (request, expected) =>
            request.Coin == expected.Coin && request.Currency == expected.Currency && request.Days == expected.Days;
        coinGeckoClientMoq.Verify(_ => _.GetCoinMarketChart(It.Is<CoinMarketChartRequest>(request => isExpectedRequest(request, coinMarketChartRequest))),
            Times.Once);

        historicalMarketDataView.Should().NotBeNull();
        historicalMarketDataView.Coin.Should().Be(coinMarketChartRequest.Coin);
        historicalMarketDataView.Currency.Should().Be(coinMarketChartRequest.Currency);
        historicalMarketDataView.Prices.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.Prices.Should().BeEquivalentTo(new List<(string, double)> { ("04/07/2023", 28477.63754439077), ("05/07/2023", 28058.665368361602) });
        historicalMarketDataView.MarketCaps.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.MarketCaps.Should().BeEquivalentTo(new List<(string, double)> { ("04/07/2023", 552996577247.077), ("05/07/2023", 544504299176.5765) });
        historicalMarketDataView.TotalVolumes.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.TotalVolumes.Should().BeEquivalentTo(new List<(string, double)> { ("04/07/2023", 13732072142.597347), ("05/07/2023", 9014016349.460764) });
    }

    [Fact]
    public async void Should_CallGetHistoricalMarketDataThrowArgumentNullException_When_GetHistoricalMarketDataQueryIsNull()
    {
        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketData(null!);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async void Should_CallGetHistoricalMarketDataThrowArgumentException_When_GetHistoricalMarketDataQueryCurrencyIsNull()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = null!,
            Days = 1
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketData(getHistoricalMarketDataQuery);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Currency");
    }

    [Fact]
    public async void Should_CallGetHistoricalMarketDataThrowArgumentException_When_GetHistoricalMarketDataQueryCoinIsNull()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = null!,
            Currency = "eur",
            Days = 1
        };

        var coinGeckoClientMoq = new Mock<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClientMoq.Object);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketData(getHistoricalMarketDataQuery);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Coin");
    }
}