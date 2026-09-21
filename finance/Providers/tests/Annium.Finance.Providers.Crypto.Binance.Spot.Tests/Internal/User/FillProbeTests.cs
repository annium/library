using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Tests.Internal.User;

/// <summary>
/// Buys and sells a small quantity for real, twice — once at market and once with a limit order priced
/// through the book — and stores every answer the venue gives about the fills.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it exists.</b> Nothing else in this venue's suite fills. Every order the trading block places
/// rests far from the market by design, so the events an execution raises, and the fields they carry, are
/// pinned offline against a payload assembled by hand from the documentation — every fill field in it
/// zero. That is a fact on the documentation axis and nothing on the verification one, and it is the last
/// thing standing between this venue and being trusted to trade.
/// </para>
/// <para>
/// <b>It is an instrument, not a regression test.</b> It records; it asserts only that the venue filled
/// what it was asked to fill, because everything else it might assert is a question the recordings are
/// being made to answer. The converters and their fixtures are written from the files afterwards.
/// </para>
/// <para>
/// <b>What it costs.</b> Two round trips of about seven units of quote currency each, which is above this
/// symbol's minimum notional with enough room that the sell back — smaller than the buy by the fee — is
/// still above it. The fee is the cost of the exercise, and it is a fraction of one unit.
/// </para>
/// <para>
/// <b>What it does to the account.</b> It buys and sells the same quantity back within seconds, so the
/// account ends as it began but for fees. This is the one place in this venue's suite that is allowed to
/// sell, it does so only to return what it just bought, and it is a probe precisely so that no recipe can
/// run it by accident.
/// </para>
/// <para>
/// Run it as: <c>PROBE_OUT=&lt;dir&gt; dotnet test … --filter-trait "block=probe" --filter-method
/// "*FillProbeTests*"</c>. Without <c>PROBE_OUT</c> it runs and records nothing, which is the one way it
/// can waste a fill.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Probe)]
public class FillProbeTests : ProvidersTestBase
{
    /// <summary>The symbol the probe trades.</summary>
    private const string Symbol = "DOTUSDT";

    /// <summary>The base asset of <see cref="Symbol"/>, which is what a fee on a buy is taken in.</summary>
    private const string BaseAsset = "DOT";

    /// <summary>The quantity step orders on <see cref="Symbol"/> must be a multiple of.</summary>
    private const decimal LotSize = 0.01m;

    /// <summary>The price step orders on <see cref="Symbol"/> must be a multiple of.</summary>
    private const decimal TickSize = 0.001m;

    /// <summary>
    /// What to spend on each buy, in quote currency. Above the symbol's minimum notional of five with
    /// enough room that the sell back, smaller by the fee, clears it too.
    /// </summary>
    private const decimal Notional = 7m;

    /// <summary>
    /// Initializes a new instance of the <see cref="FillProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public FillProbeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the Binance spot provider, so the probe signs and connects the way production does.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceSpot();
    }

    /// <summary>
    /// A market buy and the market sell that returns it, with every answer and every stream event recorded.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.HoldTimeoutMs)]
    public async Task MarketRoundTrip_RecordsEveryAnswer()
    {
        await RoundTripAsync("market", limit: false, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// A limit buy priced through the book and the limit sell that returns it, recorded the same way.
    /// </summary>
    /// <remarks>
    /// Separate from the market round trip because the two are not the same event on the wire: a limit
    /// order that fills on arrival is reported as an order that rested and then traded, and whether this
    /// venue says so in one event or two is exactly what is being recorded. Priced well through the book so
    /// it fills whole — a partially filled limit leaves a remainder resting, which the cleanup cancels.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.HoldTimeoutMs)]
    public async Task CrossingLimitRoundTrip_RecordsEveryAnswer()
    {
        await RoundTripAsync("limit", limit: true, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Buys and sells back, recording the placement, the order queried back, the trades and the stream.
    /// </summary>
    /// <param name="label">The prefix every file this run writes is named with.</param>
    /// <param name="limit">Whether to trade with limit orders priced through the book, or at market.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RoundTripAsync(string label, bool limit, CancellationToken ct)
    {
        var sp = Get<IServiceProvider>();
        var events = new ConcurrentQueue<string>();

        var settings = Settings.User;
        var signatureService = sp.CreateSignatureService(settings, settings.GetProviderKey());
        var monitor = new StatusMonitor(Get<ILogger>());

        await using var stream = sp.CreateWsApiUserStream(Endpoints.UserWsApi, 5_000, signatureService, monitor);
        stream.OnMessage += data => events.Enqueue(Encoding.UTF8.GetString(data.Span));

        // connected here means the venue accepted the signed subscription, so waiting for it is what
        // guarantees the events of the orders below are seen rather than raced
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        await SendAsync(HttpMethod.Get, "/api/v3/account", Q(), true, $"{label}.account-before", ct);

        var book = await SendAsync(
            HttpMethod.Get,
            "/api/v3/ticker/bookTicker",
            Q(("symbol", Symbol)),
            false,
            $"{label}.book",
            ct
        );
        var bid = ReadDecimal(book, "bidPrice");
        var ask = ReadDecimal(book, "askPrice");
        ask.IsGreater(0m, "the venue quoted no ask, so the probe cannot size an order");

        var qty = ToLot(Notional / ask + LotSize);

        try
        {
            // buy: at market, or priced a half percent through the ask, which is well inside the price band
            // and deep enough into the book that seven units of quote fill at once
            var buy = await PlaceAsync("BUY", qty, limit ? ToTick(ask * 1.005m) : null, $"{label}.buy", ct);

            await Task.Delay(3_000, ct);

            var buyId = ReadString(buy, "orderId");
            if (buyId.Length > 0)
                await SendAsync(
                    HttpMethod.Get,
                    "/api/v3/order",
                    Q(("symbol", Symbol), ("orderId", buyId)),
                    true,
                    $"{label}.buy.query",
                    ct
                );

            await SendAsync(
                HttpMethod.Get,
                "/api/v3/myTrades",
                Q(("symbol", Symbol), ("limit", "5")),
                true,
                $"{label}.trades",
                ct
            );

            // sell back exactly what arrived: the fee on a buy is taken in the asset bought, so the
            // quantity credited is less than the quantity filled, and asking for the filled quantity is
            // refused for insufficient balance
            var sellQty = ToLot(FilledQty(buy) - FeeInBaseAsset(buy));
            sellQty.IsGreater(0m, $"the buy filled nothing: {buy}");

            await PlaceAsync("SELL", sellQty, limit ? ToTick(bid * 0.995m) : null, $"{label}.sell", ct);

            await Task.Delay(3_000, ct);

            await SendAsync(
                HttpMethod.Get,
                "/api/v3/myTrades",
                Q(("symbol", Symbol), ("limit", "5")),
                true,
                $"{label}.trades-after",
                ct
            );
            await SendAsync(HttpMethod.Get, "/api/v3/account", Q(), true, $"{label}.account-after", ct);
        }
        finally
        {
            // a limit order that filled only in part leaves a remainder resting. Refused where nothing is
            // open, which is the ordinary outcome and not an error here
            await SendAsync(
                HttpMethod.Delete,
                "/api/v3/openOrders",
                Q(("symbol", Symbol)),
                true,
                $"{label}.cleanup",
                ct
            );

            await Task.Delay(2_000, ct);

            var dir = Environment.GetEnvironmentVariable("PROBE_OUT");
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
                var i = 0;
                foreach (var message in events)
                    await File.WriteAllTextAsync(Path.Combine(dir, $"{label}.stream.{i++:00}.json"), message, ct);
            }
        }
    }

    /// <summary>Places one order, at market or at a price, and records what the venue answers.</summary>
    /// <param name="side">The side to trade.</param>
    /// <param name="qty">The quantity, already aligned to the lot size.</param>
    /// <param name="price">The limit price, or null to trade at market.</param>
    /// <param name="label">The name the answer is saved under.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The response body as text.</returns>
    private async Task<string> PlaceAsync(string side, decimal qty, decimal? price, string label, CancellationToken ct)
    {
        var parameters = Q(
            ("symbol", Symbol),
            ("side", side),
            ("type", price is null ? "MARKET" : "LIMIT"),
            ("quantity", qty.ToString(CultureInfo.InvariantCulture)),
            ("newClientOrderId", Guid.NewGuid().ToString("N")),
            // the response type that carries the fills - the whole reason this probe exists
            ("newOrderRespType", "FULL")
        );

        if (price is not null)
        {
            parameters["price"] = price.Value.ToString(CultureInfo.InvariantCulture);
            parameters["timeInForce"] = "GTC";
        }

        return await SendAsync(HttpMethod.Post, "/api/v3/order", parameters, true, label, ct);
    }

    /// <summary>Sums the quantity of every fill the placement reported.</summary>
    /// <param name="json">The placement answer.</param>
    /// <returns>The filled quantity, or zero where the answer carries no fills.</returns>
    private static decimal FilledQty(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return 0m;

        if (doc.RootElement.TryGetProperty("executedQty", out var executed))
            return Parse(executed.ToString());

        return 0m;
    }

    /// <summary>Sums the fees the placement reported that were taken in the asset bought.</summary>
    /// <remarks>
    /// A fee charged in some other asset - the venue's own token, where the account holds one - does not
    /// reduce what can be sold back, so only fees in the base asset count here.
    /// </remarks>
    /// <param name="json">The placement answer.</param>
    /// <returns>The total fee in the base asset, or zero where there is none.</returns>
    private static decimal FeeInBaseAsset(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (
            doc.RootElement.ValueKind != JsonValueKind.Object
            || !doc.RootElement.TryGetProperty("fills", out var fills)
        )
            return 0m;

        var total = 0m;
        foreach (var fill in fills.EnumerateArray())
        {
            if (
                fill.TryGetProperty("commissionAsset", out var asset)
                && asset.GetString() == BaseAsset
                && fill.TryGetProperty("commission", out var commission)
            )
            {
                total += Parse(commission.ToString());
            }
        }

        return total;
    }

    /// <summary>Rounds a quantity down to the symbol's lot size.</summary>
    /// <param name="value">The quantity to align.</param>
    /// <returns>The aligned quantity.</returns>
    private static decimal ToLot(decimal value) => Math.Floor(value / LotSize) * LotSize;

    /// <summary>Rounds a price down to the symbol's tick size.</summary>
    /// <param name="value">The price to align.</param>
    /// <returns>The aligned price.</returns>
    private static decimal ToTick(decimal value) => Math.Floor(value / TickSize) * TickSize;

    /// <summary>Builds a query dictionary from name/value pairs.</summary>
    /// <param name="pairs">The parameters to send.</param>
    /// <returns>The dictionary.</returns>
    private static Dictionary<string, string> Q(params (string Key, string Value)[] pairs)
    {
        var result = new Dictionary<string, string>();
        foreach (var (key, value) in pairs)
            result[key] = value;

        return result;
    }

    /// <summary>Sends one request to the live exchange, saves the answer, and returns its body.</summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="path">The endpoint path to call.</param>
    /// <param name="parameters">The query parameters to send.</param>
    /// <param name="signed">Whether to append the receive window, timestamp and signature.</param>
    /// <param name="label">The name the answer is saved under.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The response body as text.</returns>
    private async Task<string> SendAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string> parameters,
        bool signed,
        string label,
        CancellationToken ct
    )
    {
        var sp = Get<IServiceProvider>();
        var factory = sp.ResolveHttpRequestFactory(Constants.GetAccountKey);
        var limiter = sp.Resolve<IRateLimiter>();

        var request = factory.New(Endpoints.HttpApi).With(method, path).Params(parameters);

        if (signed)
        {
            var settings = Settings.User;
            var signatureService = sp.CreateSignatureService(settings, settings.GetProviderKey());
            request = request.ReceiveWindow().Sign(signatureService);
        }

        var response = await request.WithRateDelay1M(limiter).RunAsync(ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        var dir = Environment.GetEnvironmentVariable("PROBE_OUT");
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(Path.Combine(dir, $"{label}.{(int)response.StatusCode}.json"), body, ct);
        }

        // not asserted: a refusal is an answer worth recording, and the cleanup must run whatever happened
        return body;
    }

    /// <summary>Reads a string property out of a flat JSON object without deserializing into a type.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or an empty string when the property is absent.</returns>
    private static string ReadString(string json, string name)
    {
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(name, out var value)
            ? value.ToString()
            : string.Empty;
    }

    /// <summary>Reads a decimal-valued property out of a flat JSON object.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or zero when the property is absent or unreadable.</returns>
    private static decimal ReadDecimal(string json, string name) => Parse(ReadString(json, name));

    /// <summary>Parses a decimal the venue sent as a string.</summary>
    /// <param name="raw">The text to parse.</param>
    /// <returns>The value, or zero where it cannot be read.</returns>
    private static decimal Parse(string raw) =>
        decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
}
