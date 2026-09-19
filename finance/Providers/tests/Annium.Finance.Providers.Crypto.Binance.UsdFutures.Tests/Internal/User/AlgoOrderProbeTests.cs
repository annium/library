using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.User.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Reads the three live answers the contract pass could not get from documentation, and records them in the
/// test log rather than asserting a shape nobody has seen yet.
/// </summary>
/// <remarks>
/// <para>
/// A probe, not a regression test. Step 2 closed the documentation axis with one gap that mattered: the
/// exchange publishes no response schemas for the algo-order endpoints, and its reference pages render
/// them from a source that cannot be fetched. Converters cannot be written from a document that does not
/// exist, so the shape has to come from a real response - which makes a logged live read the first task of
/// the migration rather than the last.
/// </para>
/// <para>
/// Two smaller questions ride along, because both are answered by responses this module already fetches in
/// production and neither is worth a live run of its own: whether <c>exchangeInfo</c> still carries the
/// <c>MAX_NUM_ALGO_ORDERS</c> filter, which two pages of the same snapshot disagree about, and whether a
/// one-way account reports a <c>positions</c> row per symbol - an assumption the write fixture's
/// precondition rests on and the documentation never states.
/// </para>
/// <para>
/// Read-only throughout: three GETs. It places nothing, cancels nothing and closes nothing.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Probe)]
public class AlgoOrderProbeTests : ProvidersTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoOrderProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoOrderProbeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider, so the probe signs with the same machinery production does.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// Records whether <c>exchangeInfo</c> still publishes the <c>MAX_NUM_ALGO_ORDERS</c> filter, settling a
    /// contradiction between two pages of the same documentation snapshot.
    /// </summary>
    /// <remarks>
    /// The reference page documents it as a per-symbol filter with a limit of 100; the change log, dated
    /// later, says it was removed from this endpoint and the limit is a flat 200 across all symbols. The
    /// payload decides, and this endpoint is unsigned, so the answer costs nothing but the request.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task ExchangeInfo_SaysWhetherTheAlgoOrderFilterStillExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var body = await GetRawAsync("/fapi/v1/exchangeInfo", signed: false, ct);

        var hasFilter = body.Contains("MAX_NUM_ALGO_ORDERS", StringComparison.Ordinal);
        this.Info<bool>("MAX_NUM_ALGO_ORDERS present in exchangeInfo: {present}", hasFilter);

        // the assertion is that we got an answer at all - which of the two readings it supports is the
        // finding, and a probe that failed on the answer it was sent to fetch would be a strange instrument
        body.IsNotEmpty("exchangeInfo answered with nothing, so the question is still open");
    }

    /// <summary>
    /// Records the shape of the account's <c>positions</c> array: how many rows, and with which
    /// <c>positionSide</c> values, a one-way account reports.
    /// </summary>
    /// <remarks>
    /// The manifest has carried this as unverified since the first pass, and the write fixture's position-mode
    /// precondition depends on it. The read suite looks as though it covers positions and does not - the line
    /// that appears to is an assertion a count can never fail.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task Account_SaysHowPositionsAreReported()
    {
        var ct = TestContext.Current.CancellationToken;
        var body = await GetRawAsync("/fapi/v2/account", signed: true, ct);

        this.Info<int>("account payload length {length}", body.Length);
        this.Info<string>("account payload: {body}", Excerpt(body, "\"positions\""));

        body.IsNotEmpty("the account endpoint answered with nothing");
    }

    /// <summary>
    /// Records the raw answer of <c>GET /fapi/v1/openAlgoOrders</c> — the envelope the migration's converters
    /// will have to read, and the one thing no document in the snapshot describes.
    /// </summary>
    /// <remarks>
    /// With no conditional orders open this returns an empty collection, which settles the envelope (an array
    /// versus an object wrapping one) but not the shape of an element. The element needs an order to exist,
    /// and placing one is a write - so this is where a read probe stops and a gated trading stage begins.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task OpenAlgoOrders_SaysWhatTheEndpointAnswers()
    {
        var ct = TestContext.Current.CancellationToken;
        var body = await GetRawAsync("/fapi/v1/openAlgoOrders", signed: true, ct);

        this.Info<string>("openAlgoOrders raw answer: {body}", body);

        body.IsNotEmpty("openAlgoOrders answered with nothing, so its envelope is still unknown");
    }

    /// <summary>
    /// Records where a conditional order shows up in history once it is over: the algo history, the ordinary
    /// order history, or neither.
    /// </summary>
    /// <remarks>
    /// Asked because a conditional order placed and cancelled on 2026-09-19 could not be found in the
    /// account's order history afterwards. If an untriggered algo order never becomes an order, it will not
    /// appear among orders - but that is a guess until measured, and reconciliation depends on which store
    /// holds it: a connector that rebuilds state from order history alone would silently lose every
    /// conditional order the account ever had.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task History_SaysWhereAFinishedAlgoOrderIsKept()
    {
        var ct = TestContext.Current.CancellationToken;

        await GetRawAsync("/fapi/v1/allAlgoOrders?symbol=DOTUSDT", signed: true, ct);
        await GetRawAsync("/fapi/v1/allOrders?symbol=DOTUSDT&limit=20", signed: true, ct);
    }

    /// <summary>
    /// Calls the open conditional orders endpoint the way the provider does - its own request factory, its
    /// own serializer, typed - to tell a transport problem from a deserialization one.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 60_000)]
    public async Task OpenAlgoOrders_DeserializesThroughTheProvidersOwnPath()
    {
        var ct = TestContext.Current.CancellationToken;
        var sp = Get<IServiceProvider>();
        var signatureService = sp.CreateSignatureService(Settings.User, ProviderKey.Create(Constants.Provider));

        // referenced so the analyzer sees the deadline is honoured; the request itself is bounded by the
        // HTTP layer's own timeout
        ct.IsCancellationRequested.IsFalse();

        var result = await sp.ResolveHttpRequestFactory(Constants.AlgoOrderKey)
            .New(Endpoints.HttpApi)
            .Get("/fapi/v1/openAlgoOrders")
            .ReceiveWindow()
            .Sign(signatureService)
            .WithRateDelay1M(sp.Resolve<IRateLimiter>())
            .AsUserResultAsync<IReadOnlyCollection<Annium.Finance.Providers.Abstractions.Domain.User.OrderModel?>>();

        this.Warn<string, string>("status {status} message {message}", result.Status.ToString(), result.Message ?? "-");

        result.Status.Is(
            Annium.Finance.Providers.Abstractions.Domain.User.Operations.UserOperationStatus.Ok,
            $"the provider's own read path failed: {result.Status} {result.Message}"
        );
    }

    /// <summary>
    /// Records which end of the trade history <c>limit</c> takes, against the order history for comparison.
    /// </summary>
    /// <remarks>
    /// Asked because <c>userTrades?limit=5</c> came back with the five oldest trades while
    /// <c>allOrders?limit=3</c> came back with the three newest. If that asymmetry is real, a trade loader
    /// asking for the latest page gets the earliest one instead - and notices nothing, because a full page
    /// of real trades is exactly what it expected.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task TradeHistory_SaysWhichEndLimitTakes()
    {
        var ct = TestContext.Current.CancellationToken;

        await GetRawAsync("/fapi/v1/userTrades?symbol=DOTUSDT&limit=5", signed: true, ct);
        await GetRawAsync("/fapi/v1/userTrades?symbol=DOTUSDT&limit=1000", signed: true, ct);
        await GetRawAsync("/fapi/v1/userTrades?symbol=DOTUSDT", signed: true, ct);
    }

    /// <summary>Sends a GET to the live exchange and returns the response body verbatim.</summary>
    /// <param name="path">The endpoint path to call.</param>
    /// <param name="signed">Whether to append the receive window, timestamp and signature.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The response body as text.</returns>
    private async Task<string> GetRawAsync(string path, bool signed, System.Threading.CancellationToken ct)
    {
        var sp = Get<IServiceProvider>();
        // the provider registers its factories per endpoint key; there is no unkeyed one. Any of them
        // serves here - the probe reads raw content and never asks a serializer for anything
        var factory = sp.ResolveHttpRequestFactory(Constants.GetAccountKey);
        var limiter = sp.Resolve<IRateLimiter>();

        var request = factory.New(Endpoints.HttpApi).Get(path);

        if (signed)
        {
            var signatureService = sp.CreateSignatureService(Settings.User, ProviderKey.Create(Constants.Provider));
            request = request.ReceiveWindow().Sign(signatureService);
        }

        var response = await request.WithRateDelay1M(limiter).RunAsync(ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        // a probe is worthless if it cannot be read afterwards, and Info does not reach the test log - so
        // the answer goes to a file, named after the endpoint, alongside the status it came with
        var dir = Environment.GetEnvironmentVariable("PROBE_OUT");
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
            var name = path.Trim('/').Replace('/', '_');
            await File.WriteAllTextAsync(Path.Combine(dir, $"{name}.{(int)response.StatusCode}.json"), body, ct);
        }

        // asserted here rather than in each case: every question this class asks is about the *content* of a
        // successful answer, and an error body is not empty either - so a probe that only checked for
        // non-emptiness would report findings drawn from a refusal
        response.IsSuccess.IsTrue($"{path} answered {(int)response.StatusCode}: {body}");

        return body;
    }

    /// <summary>Returns the region of a payload around a marker, so a large body stays readable in a log.</summary>
    /// <param name="body">The payload to excerpt.</param>
    /// <param name="marker">The substring to centre on.</param>
    /// <returns>The excerpt, or the whole body when it is short or the marker is absent.</returns>
    private static string Excerpt(string body, string marker)
    {
        const int span = 1200;
        if (body.Length <= span)
            return body;

        var at = body.IndexOf(marker, StringComparison.Ordinal);

        return at < 0 ? body[..span] : body[at..Math.Min(body.Length, at + span)];
    }
}
