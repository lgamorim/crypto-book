using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CryptoBook.CoinGecko.UnitTests;

public class CoinGeckoClientTests
{
    [Fact]
    public async void Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_SimplePriceRequestIsValid()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };
        var coinGeckoResponse = new Dictionary<string, IDictionary<string, double>>()
        {
            { "bitcoin", new Dictionary<string, double> { { "eur", 28135 }, { "usd", 30628 }, { "gbp", 24166 }, { "jpy", 4429566 } } },
            { "ethereum", new Dictionary<string, double> { { "eur", 1799 }, { "usd", 1958 }, { "gbp", 1545.29 }, { "jpy", 283242 } } },
            { "cardano", new Dictionary<string, double> { { "eur", 0.269991 }, { "usd", 0.293915 }, { "gbp", 0.231909 }, { "jpy", 42.51 } } }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string apiArgIds = "bitcoin,ethereum,cardano";
            const string apiArgCurrencies = "eur,usd,gbp,jpy";
            const string simplePriceApiUrl = $"{CoinGeckoClient.ApiRootUrl}/simple/price?ids={apiArgIds}&vs_currencies={apiArgCurrencies}";
            return message.Method.Equals(HttpMethod.Get) && message.RequestUri!.AbsoluteUri.Equals(simplePriceApiUrl);
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => isExpectedRequestMessage(message)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(coinGeckoHttpResponse);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var coinGeckoClient = new CoinGeckoClient(httpClient);
        var simplePriceResponse = await coinGeckoClient.GetSimplePrice(simplePriceRequest);

        simplePriceResponse.Should().NotBeNull();
        simplePriceResponse.HasRequestSucceeded.Should().BeTrue();
        simplePriceResponse.CryptocurrencyPrices.Should().NotBeNull().And.HaveCount(3);
        simplePriceResponse.CryptocurrencyPrices.Should().BeEquivalentTo(coinGeckoResponse);
    }
}