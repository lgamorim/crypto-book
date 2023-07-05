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

    [Fact]
    public async void Should_GetSimplePriceThrowArgumentNullException_When_SimplePriceRequestIsNull()
    {
        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePrice(null!);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async void Should_GetSimplePriceThrowArgumentException_When_SimplePriceRequestCurrenciesIsNull()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = null!
        };

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePrice(simplePriceRequest);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Currencies");
    }

    [Fact]
    public async void Should_GetSimplePriceThrowArgumentException_When_SimplePriceRequestCoinsIsNull()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = null!,
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePrice(simplePriceRequest);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Coins");
    }

    [Fact]
    public async void Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_CoinMarketChartRequestIsValid()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinGeckoResponse = new Dictionary<string, IList<double[]>>()
        {
            { "prices", new List<double[]> { new[] { 1688468488622d, 28477.63754439077 }, new[] { 1688554643000d, 28058.665368361602 } } },
            { "market_caps", new List<double[]> { new[] { 1688468488622d, 552996577247.077 }, new[] { 1688554643000d, 544504299176.5765 } } },
            { "total_volumes", new List<double[]> { new[] { 1688468488622d, 13732072142.597347 }, new[] { 1688554643000d, 9014016349.460764 } } }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string argApiId = "bitcoin";
            const string argApiCurrency = "eur";
            const string argApiDays = "1";
            const string coinMarketChartApiUrl = $"{CoinGeckoClient.ApiRootUrl}/coins/{argApiId}/market_chart?vs_currencies={argApiCurrency}&days={argApiDays}";
            return message.Method.Equals(HttpMethod.Get) && message.RequestUri!.AbsoluteUri.Equals(coinMarketChartApiUrl);
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => isExpectedRequestMessage(message)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(coinGeckoHttpResponse);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var coinGeckoClient = new CoinGeckoClient(httpClient);
        var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChart(coinMarketChartRequest);

        coinMarketChartResponse.Should().NotBeNull();
        coinMarketChartResponse.HasRequestSucceeded.Should().BeTrue();
        coinMarketChartResponse.HistoricalMarketData.Should().NotBeNull();
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().BeEquivalentTo(coinGeckoResponse["prices"]);
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().BeEquivalentTo(coinGeckoResponse["market_caps"]);
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().BeEquivalentTo(coinGeckoResponse["total_volumes"]);
    }

    [Fact]
    public async void Should_GetCoinMarketChartThrowArgumentNullException_When_CoinMarketChartRequestIsNull()
    {
        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChart(null!);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async void Should_GetCoinMarketChartThrowArgumentException_When_CoinMarketChartRequestCurrencyIsNull()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = null!
        };

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChart(coinMarketChartRequest);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Currency");
    }

    [Fact]
    public async void Should_GetCoinMarketChartThrowArgumentException_When_CoinMarketChartRequestCoinIsNull()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = null!,
            Currency = "eur"
        };

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        var coinGeckoClient = new CoinGeckoClient(httpClient);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChart(coinMarketChartRequest);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentException>();
        exception.WithMessage("Coin");
    }
}