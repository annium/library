using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.Loaders;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Drives the spot user provider's four read paths against a local HTTP server.
/// </summary>
/// <remarks>
/// These paths did not exist until 2026-09-21: the provider returned an empty success without issuing
/// a request, so a wrong key, an IP ban and an empty account were one answer. Every venue fact asserted
/// here comes from §10 of the manifest, collected from the documentation rather than from the
/// neighbouring venue - which diverges on the window, on what an unscoped order list costs, and on
/// which end of a trade page the cursor selects.
/// </remarks>
public class UserProviderReadPathTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserProviderReadPathTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public UserProviderReadPathTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the spot provider so the request factories under test resolve from their registration.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// The account load asks the documented path and maps the balances the venue answers with.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadContext_AsksTheDocumentedPathAndMapsBalances()
    {
        // arrange
        var paths = new List<string>();
        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                paths.Add(request.Url!.AbsolutePath);
                await WriteJsonAsync(
                    response,
                    @"{ ""balances"": [ { ""asset"": ""BTC"", ""free"": ""1.5"", ""locked"": ""0.25"" } ] }"
                );
            }
        );

        // act
        var result = await CreateProvider(server).LoadContextAsync();

        // assert
        paths.Single().Is("/api/v3/account", "the account path is not the one the contract names");
        result.Status.Is(UserOperationStatus.Ok);

        var context = result.Data.NotNull();
        var asset = context.Assets.Single();
        asset.Resource.Is("BTC");
        asset.Free.Is(1.5m);
        asset.Locked.Is(0.25m);
        context.Positions.IsEmpty("a spot account reported a position");
    }

    /// <summary>
    /// The open-order load asks the documented path, and asks it for every symbol.
    /// </summary>
    /// <remarks>
    /// Unscoped deliberately - the interface asks for every open order - and the manifest records what
    /// that costs: 80 against 6 when a symbol is sent. The assertion is that no symbol is sent, because
    /// sending one would answer a different question cheaply rather than this one.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadOpenOrders_AsksForEverySymbol()
    {
        // arrange
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

        // act
        var result = await CreateProvider(server).LoadOpenOrdersAsync();

        // assert
        paths.Single().Is("/api/v3/openOrders");
        queries.Single()["symbol"].IsDefault("the open-order load scoped itself to one symbol");
        result.Status.Is(UserOperationStatus.Ok);
    }

    /// <summary>
    /// Asked for the latest orders, the provider sends no window and takes what the venue calls recent.
    /// </summary>
    /// <remarks>
    /// The venue returns the most recent rows when no cursor is sent, so "latest" is the absence of a
    /// window rather than a window ending now. Asserted because the neighbouring venue's trade endpoint
    /// behaves the opposite way, and a loader that asks the wrong way notices nothing: a full page of
    /// real rows is exactly what it expected to see.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadOrders_WithoutASince_AsksForTheMostRecentPage()
    {
        // arrange
        var queries = new List<NameValueCollection>();
        var paths = new List<string>();
        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                paths.Add(request.Url!.AbsolutePath);
                queries.Add(request.QueryString);
                await WriteJsonAsync(response, "[]");
            }
        );

        // act
        await CreateProvider(server).LoadOrdersAsync("BTCUSDT", null);

        // assert
        paths.Single().Is("/api/v3/allOrders");

        var query = queries.Single();
        query["symbol"].Is("BTCUSDT");
        query["limit"].Is("1000", "the page size is not the documented maximum");
        query["startTime"].IsDefault("the latest page was asked for with a window");
        query["endTime"].IsDefault("the latest page was asked for with a window");
    }

    /// <summary>
    /// Asked for history, the provider walks forward in windows the venue accepts.
    /// </summary>
    /// <remarks>
    /// Twenty-four hours, which is the cap this venue states and a quarter of what the neighbour
    /// allows. The fixture spans three windows so the walk between them is exercised rather than
    /// assumed, and each window's start is asserted against the previous one's end.
    /// </remarks>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadOrders_WithASince_WalksForwardInWindowsTheVenueAccepts()
    {
        // arrange - three days back, so the walk needs three windows and part of a fourth
        var day = (long)TimeSpan.FromHours(24).TotalMilliseconds;
        var since = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (day * 3);
        var windows = new List<(long Start, long End)>();

        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                windows.Add(
                    (
                        long.Parse(request.QueryString["startTime"]!, CultureInfo.InvariantCulture),
                        long.Parse(request.QueryString["endTime"]!, CultureInfo.InvariantCulture)
                    )
                );
                await WriteJsonAsync(response, "[]");
            }
        );

        // act
        var result = await CreateProvider(server).LoadOrdersAsync("BTCUSDT", since);

        // assert
        result.Status.Is(UserOperationStatus.Ok);
        windows.Count.IsGreaterOrEqual(3, "three days of history were not walked in more than two windows");

        foreach (var window in windows)
            (window.End - window.Start).IsLessOrEqual(day, "a window longer than the venue accepts was asked for");

        for (var i = 1; i < windows.Count; i++)
            windows[i].Start.Is(windows[i - 1].End, "a window did not start where the previous one ended");
    }

    /// <summary>
    /// The trade paths ask the documented path, with the same two shapes.
    /// </summary>
    /// <param name="since">The moment to load from, or null for the latest page.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData(1_700_000_000_000L)]
    public async Task LoadTrades_AsksTheDocumentedPath(long? since)
    {
        // arrange
        var paths = new List<string>();
        await using var server = this.RunHttpServer(
            async (request, response) =>
            {
                paths.Add(request.Url!.AbsolutePath);
                await WriteJsonAsync(response, "[]");
            }
        );

        // act
        var result = await CreateProvider(server).LoadTradesAsync("BTCUSDT", since);

        // assert
        result.Status.Is(UserOperationStatus.Ok);
        paths.Count.IsGreaterOrEqual(1, "no trade request was made");
        paths.Distinct().Single().Is("/api/v3/myTrades");
    }

    /// <summary>
    /// A refused load is a failure on every one of the four paths, and not an empty account.
    /// </summary>
    /// <remarks>
    /// The point of the whole step, stated as one test. Until these paths existed, every one of them
    /// answered `Ok` with nothing in it, so a wrong key, an IP ban and an empty account were the same
    /// answer. Three of the four return collections, which is the shape that used to swallow an
    /// exchange error upstream - so this is also the regression guard for that fix at this venue's
    /// call sites.
    /// </remarks>
    /// <param name="path">The read path under test, named for the failure message.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData("context")]
    [InlineData("openOrders")]
    [InlineData("orders")]
    [InlineData("trades")]
    public async Task EveryReadPath_ThatIsRefused_IsAFailureAndNotAnEmptyAccount(string path)
    {
        // arrange
        await using var server = this.RunHttpServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes(@"{ ""code"": -2015, ""msg"": ""Invalid API-key."" }");
                response.StatusCode(HttpStatusCode.Unauthorized);
                response.ContentType = MediaTypeNames.Application.Json;
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );
        var provider = CreateProvider(server);

        // act
        var status = path switch
        {
            "context" => (await provider.LoadContextAsync()).Status,
            "openOrders" => (await provider.LoadOpenOrdersAsync()).Status,
            "orders" => (await provider.LoadOrdersAsync("BTCUSDT", null)).Status,
            _ => (await provider.LoadTradesAsync("BTCUSDT", null)).Status,
        };

        // assert
        status.IsNot(UserOperationStatus.Ok, $"a refused {path} load read as success");
    }

    /// <summary>
    /// Writes a JSON body with a 200 status.
    /// </summary>
    /// <param name="response">The response to write to.</param>
    /// <param name="json">The body.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WriteJsonAsync(HttpListenerResponse response, string json)
    {
        var payload = Encoding.UTF8.GetBytes(json);
        response.StatusCode(HttpStatusCode.OK);
        response.ContentType = MediaTypeNames.Application.Json;
        response.ContentLength64 = payload.Length;
        await response.OutputStream.WriteAsync(payload);
    }

    /// <summary>
    /// Builds a spot user provider pointed at the given local server.
    /// </summary>
    /// <param name="server">The local server standing in for the exchange.</param>
    /// <returns>The provider under test.</returns>
    private UserProvider CreateProvider(IServer server)
    {
        var sp = Get<IServiceProvider>();
        var reload = new CompositeLoaderConfig(1, 2, 5, 0, 0);
        var config = new UserConfig
        {
            Provider = Constants.Provider,
            Key = "some_key",
            Secret = "some_secret",
            HttpApi = server.HttpUri(),
            WsApi = new Uri("wss://unused"),
            ListenKeyUriPath = "/unused/",
            ListenKey = new ListenKeyConfiguration(1000, 1000),
            ReloadContext = reload,
            ReloadOrders = reload,
            ReloadTrades = reload,
        };

        return new UserProvider(
            config,
            new StubSignatureService(),
            sp.ResolveHttpRequestFactory(Constants.GetAccountKey),
            sp.ResolveHttpRequestFactory(Constants.GetOrderKey),
            sp.ResolveHttpRequestFactory(Constants.GetTradeKey),
            sp.Resolve<IRateLimiter>(),
            Logger
        );
    }

    /// <summary>
    /// A signature service that signs nothing, so a test about read paths is not also a test about HMAC.
    /// </summary>
    private sealed class StubSignatureService : ISignatureService
    {
        /// <summary>Gets a fixed server time.</summary>
        public long ServerTime => 1_700_000_000_000;

        /// <summary>Returns a fixed key.</summary>
        /// <returns>A fixed value.</returns>
        public string GetKey() => "some_key";

        /// <summary>Returns a fixed signature.</summary>
        /// <param name="data">Ignored.</param>
        /// <returns>A fixed value.</returns>
        public string GetSignature(string data) => "signature";
    }
}
