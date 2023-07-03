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
}