using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Market;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.Market;

/// <summary>
/// Drives the spot market provider's read paths against a local HTTP server.
/// </summary>
/// <remarks>
/// Spot and futures look alike here and are not: the notional filter has a different type name and a
/// different field, the tradability rules are different, and spot alone requires the <c>SPOT</c> permission.
/// A test on the futures side says nothing about any of it, which is why these exist separately rather than
/// being shared.
/// </remarks>
public class MarketProviderReadPathTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketProviderReadPathTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper used to capture test logs.</param>
    public MarketProviderReadPathTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, so the serializers and request factories are the registered ones.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// The weight ceiling comes from the response rather than the value compiled in at registration.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_TakesTheWeightCeilingFromTheResponse()
    {
        // arrange
        var limiter = new RecordingRateLimiter();
        await using var server = ServeJson(ExchangeInfo(weightLimit: 1234));
        var provider = CreateProvider(server, limiter);

        // act
        var result = await provider.LoadContextAsync();

        // assert
        result.Status.Is(MarketOperationStatus.Ok);
        limiter.Limits.IsEqual(new[] { 1234 });
    }

    /// <summary>
    /// A symbol the exchange is not trading is dropped. Spot has five symbol statuses and only
    /// <c>TRADING</c> is one this provider can offer.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_DropsASymbolThatIsNotTrading()
    {
        // arrange
        await using var server = ServeJson(ExchangeInfo(status: "HALT"));
        var provider = CreateProvider(server);

        // act
        var context = (await provider.LoadContextAsync()).Data.NotNull();

        // assert
        context.Instruments.IsEmpty("a halted symbol must not be offered as tradable");
    }

    /// <summary>
    /// A symbol whose <c>isSpotTradingAllowed</c> flag is false is dropped, even while its permissions still
    /// list <c>SPOT</c>.
    /// </summary>
    /// <remarks>
    /// The two are separate checks and are varied separately here, which was not true of the first version
    /// of this fixture: it turned both off at once, so removing either check left the other catching the
    /// case and neither was individually pinned. A mutation dropping the flag check survived, which is what
    /// exposed it. Two mechanisms enforcing one contract need two fixtures, or they cover for each other.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_DropsASymbolWithSpotTradingDisallowed()
    {
        // arrange
        await using var server = ServeJson(ExchangeInfo(spotAllowed: false, permission: "SPOT"));
        var provider = CreateProvider(server);

        // act
        var context = (await provider.LoadContextAsync()).Data.NotNull();

        // assert
        context.Instruments.IsEmpty("a symbol with spot trading disallowed must not be offered");
    }

    /// <summary>
    /// A symbol whose permissions do not include <c>SPOT</c> is dropped, even while its
    /// <c>isSpotTradingAllowed</c> flag is true. This has no futures counterpart at all — there, the
    /// equivalent question is the contract type.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_DropsASymbolWithoutSpotPermission()
    {
        // arrange
        await using var server = ServeJson(ExchangeInfo(permission: "MARGIN"));
        var provider = CreateProvider(server);

        // act
        var context = (await provider.LoadContextAsync()).Data.NotNull();

        // assert
        context.Instruments.IsEmpty("a symbol without the SPOT permission must not be offered");
    }

    /// <summary>
    /// The notional bounds come from spot's <c>NOTIONAL</c> filter, which carries both a minimum and a
    /// maximum — where futures has <c>MIN_NOTIONAL</c>, one field, and a synthesized maximum.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_ReadsBothNotionalBounds()
    {
        // arrange
        await using var server = ServeJson(ExchangeInfo());
        var provider = CreateProvider(server);

        // act
        var context = (await provider.LoadContextAsync()).Data.NotNull();

        // assert
        var instrument = context.Instruments.Single();
        instrument.MinSum.Is(5m);
        instrument.MaxSum.Is(9_000_000m, "spot reports a maximum notional and it must not be discarded");
    }

    /// <summary>
    /// Candle loading pages: each request asks from the minute after the last candle received. Spot runs the
    /// same base implementation as futures, but the request it builds around it is its own.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadCandles_AsksFromWhereTheLastPageEnded()
    {
        // arrange - one candle per response, so every page boundary is a request we can observe
        var start = Instant.FromUnixTimeMilliseconds(1_700_000_000_000);
        var minute = Duration.FromMinutes(1);
        var requested = new List<long>();

        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                var from = long.Parse(request.QueryString["startTime"]!, CultureInfo.InvariantCulture);
                requested.Add(from);
                await WriteJsonAsync(response, $"[[{from},\"1\",\"2\",\"0.5\",\"1.5\",\"10\"]]");
            }
        );
        var provider = CreateProvider(server);

        // act
        var batches = new List<int>();
        await foreach (
            var batch in provider.LoadCandlesAsync(
                "BTCUSDT",
                start,
                start + minute * 3,
                TestContext.Current.CancellationToken
            )
        )
            batches.Add(batch.Data.NotNull().Count);

        // assert
        requested.IsEqual(
            new[]
            {
                start.ToUnixTimeMilliseconds(),
                (start + minute).ToUnixTimeMilliseconds(),
                (start + minute * 2).ToUnixTimeMilliseconds(),
            }
        );
        batches.Count.Is(3);
    }

    /// <summary>
    /// Exchange info that comes back refused is a failure, not an exchange with no instruments on it. The
    /// distinction matters more here than elsewhere: an empty instrument set is a state a connector can reach
    /// legitimately, so a silent failure would be indistinguishable from a venue that had listed nothing.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_ThatIsRefused_IsAFailureAndNotAnEmptyExchange()
    {
        // arrange
        await using var server = this.RunHttpServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes(@"{ ""code"": -1003, ""msg"": ""Too many requests."" }");
                response.StatusCode(HttpStatusCode.TooManyRequests);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );
        var provider = CreateProvider(server);

        // act
        var result = await provider.LoadContextAsync();

        // assert
        result.Status.IsNot(MarketOperationStatus.Ok, "a refused exchange info load must not read as success");
        result.Data.IsDefault("an exchange that could not be read is not an exchange with no instruments");
    }

    /// <summary>
    /// The candle request carries every parameter the contract names, and asks the documented path.
    /// </summary>
    /// <remarks>
    /// Only the cursor was asserted before, because the paging test needed it and nothing else needed
    /// anything. That leaves the other three free to be wrong: a mutated symbol fetches another
    /// instrument's candles and still returns a full page, which is exactly what the caller expected
    /// to see. The path is here for the same reason - the local server answers anything, so a typo or
    /// a wrong version is invisible offline and surfaces only against the venue.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadCandles_AsksTheDocumentedPathWithEveryParameter()
    {
        // arrange
        var start = Instant.FromUnixTimeMilliseconds(1_700_000_000_000);
        var paths = new List<string>();
        var queries = new List<NameValueCollection>();

        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                paths.Add(request.Url!.AbsolutePath);
                queries.Add(request.QueryString);
                await WriteJsonAsync(response, "[]");
            }
        );
        var provider = CreateProvider(server);

        // act
        await foreach (
            var _ in provider.LoadCandlesAsync(
                "BTCUSDT",
                start,
                start + Duration.FromMinutes(1),
                TestContext.Current.CancellationToken
            )
        ) { }

        // assert
        paths.Count.IsGreaterOrEqual(1, "no candle request was made");
        paths[0].Is("/api/v3/klines", "the candle path is not the one the contract names");

        var query = queries[0];
        query["symbol"].Is("BTCUSDT", "the candle request asked for another instrument");
        query["interval"].Is("1m", "the candle request asked for another interval");
        query["limit"].IsNotDefault("the candle request sent no page size");
        query["startTime"].Is(start.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// A refused candle page is a failure, and an empty one is not.
    /// </summary>
    /// <remarks>
    /// The success type here is a collection, which is the shape that used to swallow an exchange
    /// error and hand the caller an empty list instead. The futures venue re-pinned that at its own
    /// call sites after the upstream fix; this path never was. The two outcomes are asserted together
    /// on purpose: what matters is not that a refusal fails, it is that a refusal and a quiet minute
    /// do not look the same. They do not, and not in the way first assumed - a window the venue
    /// answers with no candles yields **no batch at all**, while a refusal yields one carrying a
    /// status the caller can act on. Distinguishable, which is the property; worth writing down,
    /// because "empty answer" and "empty batch" are not the same thing here.
    ///
    /// Each provider gets a limiter of its own, because a refusal is not free of consequences: it
    /// stands the limiter down, and the container's is shared, so the next load is refused locally
    /// before it is sent and answers TooManyRequests without the server hearing about it. That is the
    /// limiter doing its job, and it would have made this test assert the wrong thing about the
    /// provider. Found by asserting on the status rather than on a boolean, which is the only reason
    /// the cause was visible at all.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadCandles_ThatIsRefused_IsAFailureAndNotAnEmptyPage()
    {
        // arrange
        var start = Instant.FromUnixTimeMilliseconds(1_700_000_000_000);

        await using var refusing = this.RunHttpServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes(@"{ ""code"": -1003, ""msg"": ""Too many requests."" }");
                response.StatusCode(HttpStatusCode.TooManyRequests);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );
        await using var quiet = this.RunHttpServer(async (_, response) => await WriteJsonAsync(response, "[]"));

        // act
        var refused = await ReadFirstBatchAsync(CreateProvider(refusing, new AlwaysAllowingRateLimiter()), start);
        var empty = await ReadFirstBatchAsync(CreateProvider(quiet, new AlwaysAllowingRateLimiter()), start);

        // assert
        refused
            .NotNull("a refused candle load yielded nothing, so nothing can act on it")
            .Status.IsNot(MarketOperationStatus.Ok, "a refused candle page read as success");
        empty.IsDefault("a window the venue answered with no candles yielded a batch");
    }

    /// <summary>
    /// Reads the first batch a candle load yields, or null when it yields none.
    /// </summary>
    /// <param name="provider">The provider to read through.</param>
    /// <param name="start">The instant to read from.</param>
    /// <returns>The first batch, or null.</returns>
    private static async Task<MarketResult<IReadOnlyCollection<CandleModel>?>?> ReadFirstBatchAsync(
        MarketProvider provider,
        Instant start
    )
    {
        await foreach (
            var batch in provider.LoadCandlesAsync(
                "BTCUSDT",
                start,
                start + Duration.FromMinutes(1),
                TestContext.Current.CancellationToken
            )
        )
            return batch;

        return null;
    }

    /// <summary>
    /// The exchange info request asks the documented path.
    /// </summary>
    /// <remarks>
    /// Same reason as the candle path above: the test server answers whatever it is asked, so the one
    /// string that has to match the venue is the one nothing was checking. This venue has already lost
    /// a live run to exactly that - a path pinned at a version the exchange had moved on from.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_AsksTheDocumentedPath()
    {
        // arrange
        var paths = new List<string>();

        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                paths.Add(request.Url!.AbsolutePath);
                await WriteJsonAsync(response, @"{ ""rateLimits"": [], ""symbols"": [] }");
            }
        );
        var provider = CreateProvider(server);

        // act
        await provider.LoadContextAsync();

        // assert
        paths.Count.Is(1, "the exchange info load did not make exactly one request");
        paths[0].Is("/api/v3/exchangeInfo", "the exchange info path is not the one the contract names");
    }

    /// <summary>
    /// A rate limiter that permits everything and remembers nothing, so a test about the provider is
    /// not also a test about the limiter's memory of a refusal.
    /// </summary>
    private sealed class AlwaysAllowingRateLimiter : IRateLimiter
    {
        /// <summary>Always allows the request.</summary>
        /// <returns>Always true.</returns>
        public bool CanExecute() => true;

        /// <summary>Ignores the reported ceiling.</summary>
        /// <param name="limit">Ignored.</param>
        public void UpdateLimit(int limit) { }

        /// <summary>Ignores the reported weight.</summary>
        /// <param name="weight">Ignored.</param>
        public void UsedWeight(int weight) { }

        /// <summary>Ignores the requested pause, which is the whole point.</summary>
        /// <param name="duration">Ignored.</param>
        public void Block(TimeSpan duration) { }

        /// <summary>Holds nothing to release.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Builds a market provider pointed at the given local server.
    /// </summary>
    /// <param name="server">The local server standing in for the exchange.</param>
    /// <param name="limiter">The rate limiter to hand the provider; a real one when not given.</param>
    /// <returns>The provider under test.</returns>
    private MarketProvider CreateProvider(IServer server, IRateLimiter? limiter = null)
    {
        var sp = Get<IServiceProvider>();
        var config = new MarketConfig
        {
            Provider = Constants.Provider,
            HttpApi = server.HttpUri(),
            WsApi = new Uri("wss://unused"),
            WsUriPath = "/unused",
        };

        return new MarketProvider(
            config,
            sp.ResolveHttpRequestFactory(Constants.ExchangeInfoKey),
            sp.ResolveHttpRequestFactory(Constants.CandleKey),
            limiter ?? sp.Resolve<IRateLimiter>(),
            Logger
        );
    }

    /// <summary>Starts a server answering every request with the given JSON body.</summary>
    /// <param name="json">The body to answer with.</param>
    /// <returns>The running server.</returns>
    private IServer ServeJson(string json) =>
        this.RunHttpServer(async (_, response) => await WriteJsonAsync(response, json));

    /// <summary>Writes a JSON body and a 200 to the response.</summary>
    /// <param name="response">The response to write to.</param>
    /// <param name="json">The body to write.</param>
    /// <returns>A task representing the write.</returns>
    private static async Task WriteJsonAsync(HttpListenerResponse response, string json)
    {
        var payload = Encoding.UTF8.GetBytes(json);
        response.StatusCode(HttpStatusCode.OK);
        response.ContentType = MediaTypeNames.Application.Json;
        response.ContentLength64 = payload.Length;
        await response.OutputStream.WriteAsync(payload);
    }

    /// <summary>
    /// Builds a spot exchange-info payload carrying one symbol, varying only what a test is about.
    /// </summary>
    /// <param name="weightLimit">The request-weight limit the response reports.</param>
    /// <param name="status">The symbol's status.</param>
    /// <param name="spotAllowed">The symbol's <c>isSpotTradingAllowed</c> flag.</param>
    /// <param name="permission">The single permission the symbol carries.</param>
    /// <returns>The payload, as JSON.</returns>
    private static string ExchangeInfo(
        int weightLimit = 6000,
        string status = "TRADING",
        bool spotAllowed = true,
        string permission = "SPOT"
    )
    {
        var permissions = $@"[ ""{permission}"" ]";

        return $@"{{
            ""rateLimits"": [ {{ ""rateLimitType"": ""REQUEST_WEIGHT"", ""interval"": ""MINUTE"", ""intervalNum"": 1, ""limit"": {weightLimit} }} ],
            ""symbols"": [ {{
                ""symbol"": ""BTCUSDT"",
                ""status"": ""{status}"",
                ""baseAsset"": ""BTC"",
                ""baseAssetPrecision"": 8,
                ""quoteAsset"": ""USDT"",
                ""quoteAssetPrecision"": 8,
                ""isSpotTradingAllowed"": {(spotAllowed ? "true" : "false")},
                ""permissions"": {permissions},
                ""filters"": [
                    {{ ""minPrice"": ""0.01"", ""maxPrice"": ""1000000"", ""filterType"": ""PRICE_FILTER"", ""tickSize"": ""0.01"" }},
                    {{ ""stepSize"": ""0.00001"", ""filterType"": ""LOT_SIZE"", ""maxQty"": ""9000"", ""minQty"": ""0.00001"" }},
                    {{ ""stepSize"": ""0.00001"", ""filterType"": ""MARKET_LOT_SIZE"", ""maxQty"": ""100"", ""minQty"": ""0.00001"" }},
                    {{ ""limit"": 200, ""filterType"": ""MAX_NUM_ORDERS"" }},
                    {{ ""minNotional"": ""5.0"", ""maxNotional"": ""9000000"", ""filterType"": ""NOTIONAL"" }}
                ]
            }} ]
        }}";
    }

    /// <summary>A rate limiter that records what limits it was given, and permits everything.</summary>
    private sealed class RecordingRateLimiter : IRateLimiter
    {
        /// <summary>Does nothing; this fake never refuses.</summary>
        /// <param name="duration">Ignored.</param>
        public void Block(TimeSpan duration) { }

        /// <summary>Gets every limit this limiter has been told to use, in order.</summary>
        public List<int> Limits { get; } = [];

        /// <summary>Always allows a request.</summary>
        /// <returns>Always <see langword="true"/>.</returns>
        public bool CanExecute() => true;

        /// <summary>Records the limit.</summary>
        /// <param name="limit">The limit reported by the exchange.</param>
        public void UpdateLimit(int limit) => Limits.Add(limit);

        /// <summary>Ignores the reported weight; this limiter never throttles.</summary>
        /// <param name="weight">Ignored.</param>
        public void UsedWeight(int weight) { }

        /// <summary>Nothing to release.</summary>
        public void Dispose() { }
    }
}
