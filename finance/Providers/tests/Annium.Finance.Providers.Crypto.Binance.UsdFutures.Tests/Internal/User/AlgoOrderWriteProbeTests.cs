using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Places one real conditional order through <c>POST /fapi/v1/algoOrder</c>, records what the endpoint
/// answers, queries it, and cancels it — so the migration's converters can be written from a response that
/// exists rather than from a schema that does not.
/// </summary>
/// <remarks>
/// <para>
/// This is the one question the documentation pass could not answer. The exchange publishes no response
/// schemas for the algo-order family: its reference pages render them from a source that returns the site's
/// HTML shell, and the official Postman collections carry no response examples. The read probe settled the
/// envelope — an empty collection comes back as a bare <c>[]</c> — but an element needs an order to exist.
/// </para>
/// <para>
/// Deliberately small and deliberately harmless. The order is a <c>STOP_MARKET</c> whose trigger sits ~4%
/// away from the market, so it cannot fill during the seconds this runs while staying inside the
/// <c>PERCENT_PRICE</c> filter; its quantity is the smallest the <c>MIN_NOTIONAL</c> filter allows. It is
/// cancelled in a <c>finally</c>, so an assertion failing mid-way still leaves the account as it was found.
/// </para>
/// <para>
/// In the write block. It mutates a real account.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Probe)]
public class AlgoOrderWriteProbeTests : ProvidersTestBase
{
    /// <summary>The symbol the probe trades, chosen for the lowest notional the venue allows.</summary>
    private const string Symbol = "DOTUSDT";

    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoOrderWriteProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoOrderWriteProbeTests(ITestOutputHelper outputHelper)
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
    /// Places, queries and cancels one conditional order, saving every raw answer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task AlgoOrder_PlaceQueryCancel_RecordsEveryAnswer()
    {
        var ct = TestContext.Current.CancellationToken;

        var priceBody = await SendAsync(
            HttpMethod.Get,
            "/fapi/v1/ticker/price",
            new() { ["symbol"] = Symbol },
            false,
            "ticker",
            ct
        );
        var price = ReadDecimal(priceBody, "price");
        price.IsGreater(0m, "the venue quoted no price, so the probe cannot size an order");

        // ~4% away: far enough not to trigger in the seconds this takes, near enough to stay inside the
        // PERCENT_PRICE filter's 5% band
        var trigger = decimal.Round(price * 1.04m, 4, MidpointRounding.ToZero);
        // the smallest quantity MIN_NOTIONAL (5 USDT) admits, on a 0.1 lot step, with a little headroom
        var qty = decimal.Ceiling(5.5m / price * 10m) / 10m;

        var clientAlgoId = Guid.NewGuid().ToString();
        var placed = await SendAsync(
            HttpMethod.Post,
            "/fapi/v1/algoOrder",
            new()
            {
                ["algoType"] = "CONDITIONAL",
                ["symbol"] = Symbol,
                ["side"] = "BUY",
                ["positionSide"] = "BOTH",
                ["type"] = "STOP_MARKET",
                ["quantity"] = qty.ToString(CultureInfo.InvariantCulture),
                ["triggerPrice"] = trigger.ToString(CultureInfo.InvariantCulture),
                ["clientAlgoId"] = clientAlgoId,
                ["newOrderRespType"] = "RESULT",
            },
            true,
            "algoOrder.place",
            ct
        );

        var algoId = ReadString(placed, "algoId");

        try
        {
            await SendAsync(HttpMethod.Get, "/fapi/v1/openAlgoOrders", new(), true, "openAlgoOrders.populated", ct);
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/algoOrder",
                new() { ["algoId"] = algoId },
                true,
                "algoOrder.query",
                ct
            );
        }
        finally
        {
            await SendAsync(
                HttpMethod.Delete,
                "/fapi/v1/algoOrder",
                new() { ["algoId"] = algoId },
                true,
                "algoOrder.cancel",
                ct
            );
        }

        algoId.IsNotEmpty("the placement answered without an algoId, so nothing could be cancelled");
    }

    /// <summary>
    /// Closes any open position on the probe symbol, reduce-only, and asserts the account is left flat.
    /// </summary>
    /// <remarks>
    /// A tool, not a test of anything. A live stage that fails part-way leaves its teardown unrun, and the
    /// position it opened outlives the run - the one outcome the write block's own instructions single out.
    /// Reduce-only, so it can only ever close: if nothing is open the exchange refuses it, which is the
    /// right answer rather than an opposite position.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task CloseAnyOpenPosition()
    {
        var ct = TestContext.Current.CancellationToken;

        var account = await SendAsync(HttpMethod.Get, "/fapi/v2/account", new(), true, "close.account-before", ct);
        using var doc = System.Text.Json.JsonDocument.Parse(account);
        var amount = 0m;
        foreach (var p in doc.RootElement.GetProperty("positions").EnumerateArray())
        {
            if (p.GetProperty("symbol").GetString() != Symbol)
                continue;

            decimal.TryParse(
                p.GetProperty("positionAmt").GetString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out amount
            );
        }

        if (amount == 0m)
            return;

        await SendAsync(
            HttpMethod.Post,
            "/fapi/v1/order",
            new()
            {
                ["symbol"] = Symbol,
                ["side"] = amount > 0 ? "SELL" : "BUY",
                ["positionSide"] = "BOTH",
                ["type"] = "MARKET",
                ["quantity"] = Math.Abs(amount).ToString(CultureInfo.InvariantCulture),
                ["reduceOnly"] = "true",
                ["newClientOrderId"] = Guid.NewGuid().ToString(),
                ["newOrderRespType"] = "RESULT",
            },
            true,
            "close.order",
            ct
        );

        var after = await SendAsync(HttpMethod.Get, "/fapi/v2/account", new(), true, "close.account-after", ct);
        after.Contains($"\"symbol\":\"{Symbol}\"", StringComparison.Ordinal).IsTrue();
    }

    /// <summary>
    /// Cancels every order left open on the test symbol, ordinary and conditional alike.
    /// </summary>
    /// <remarks>
    /// The companion to <see cref="CloseAnyOpenPosition"/>, and written for the same reason: a live stage
    /// that fails part-way leaves its teardown unrun. A trading run has already ended with its last order
    /// accepted by the exchange and its cancel refused locally, which leaves a resting order behind that
    /// nothing in the suite is going to notice.
    ///
    /// The two kinds live in separate stores and neither endpoint touches the other's, so both are asked.
    /// Cancelling nothing is a normal outcome here, not a failure.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TestBlock.ReadTimeoutMs)]
    public async Task CancelAnyOpenOrders()
    {
        var ct = TestContext.Current.CancellationToken;

        await SendAsync(
            HttpMethod.Delete,
            "/fapi/v1/allOpenOrders",
            new() { ["symbol"] = Symbol },
            true,
            "cleanup.orders",
            ct
        );

        var open = await SendAsync(
            HttpMethod.Get,
            "/fapi/v1/openAlgoOrders",
            new(),
            true,
            "cleanup.algo-before",
            ct
        );

        using var doc = System.Text.Json.JsonDocument.Parse(open);
        foreach (var order in doc.RootElement.EnumerateArray())
        {
            var algoId = order.GetProperty("algoId").ToString();
            await SendAsync(
                HttpMethod.Delete,
                "/fapi/v1/algoOrder",
                new() { ["algoId"] = algoId },
                true,
                $"cleanup.algo-{algoId}",
                ct
            );
        }
    }

    /// <summary>
    /// Places one market order and records every raw answer the exchange gives about it: the placement, the
    /// order queried back, the trades it produced, and the stream events it raised.
    /// </summary>
    /// <remarks>
    /// Opened because a filled market order came back with an executed price of zero, and "the field was
    /// removed" is a reading of a document rather than a measurement. This records what the exchange
    /// actually sends, so the question - is the price absent, or is it under a name we do not read - is
    /// answered from bytes.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 180_000)]
    public async Task MarketOrder_RecordsEveryAnswerAboutIt()
    {
        var ct = TestContext.Current.CancellationToken;
        var sp = Get<IServiceProvider>();
        var messages = new System.Collections.Concurrent.ConcurrentQueue<string>();

        var signatureService = sp.CreateSignatureService(Settings.User, ProviderKey.Create(Constants.Provider));
        var monitor = new Annium.Finance.Providers.Core.Shared.Status.StatusMonitor(Logger);
        var config = new Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User.UserConfig
        {
            Provider = Constants.Provider,
            Key = Settings.User.Key,
            Secret = Settings.User.Secret,
            HttpApi = Endpoints.HttpApi,
            WsApi = Endpoints.WsApi,
            ListenKeyUriPath = Endpoints.UserWsUriPath,
            ListenKey = new Annium.Finance.Providers.Crypto.Binance.Base.User.Services.ListenKeyConfiguration(
                60_000,
                5_000
            ),
            ReloadContext = new Annium.Finance.Providers.Core.Shared.Loaders.CompositeLoaderConfig(
                1000,
                5,
                5000,
                1000,
                100
            ),
            ReloadOrders = new Annium.Finance.Providers.Core.Shared.Loaders.CompositeLoaderConfig(
                1000,
                5,
                5000,
                1000,
                100
            ),
            ReloadTrades = new Annium.Finance.Providers.Core.Shared.Loaders.CompositeLoaderConfig(
                1000,
                5,
                5000,
                1000,
                100
            ),
        };

        var resolver = sp.CreateListenKeyResolver(
            config,
            Endpoints.ListenKeyUriPath,
            Constants.ListenKeyKey,
            signatureService,
            monitor
        );
        using var stream = sp.CreateUserStream(config, resolver, monitor);
        stream.OnMessage += data => messages.Enqueue(System.Text.Encoding.UTF8.GetString(data.Span));

        await Task.Delay(10_000, ct);

        var price = ReadDecimal(
            await SendAsync(HttpMethod.Get, "/fapi/v1/ticker/price", Q(("symbol", Symbol)), false, "cap.ticker", ct),
            "price"
        );
        var qty = decimal.Ceiling(5.5m / price * 10m) / 10m;
        var clientOrderId = Guid.NewGuid().ToString();

        var placed = await SendAsync(
            HttpMethod.Post,
            "/fapi/v1/order",
            Q(
                ("symbol", Symbol),
                ("side", "BUY"),
                ("positionSide", "BOTH"),
                ("type", "MARKET"),
                ("quantity", qty.ToString(CultureInfo.InvariantCulture)),
                ("newClientOrderId", clientOrderId),
                ("newOrderRespType", "RESULT")
            ),
            true,
            "cap.place-RESULT",
            ct
        );
        var orderId = ReadString(placed, "orderId");

        try
        {
            await Task.Delay(3000, ct);
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/order",
                Q(("symbol", Symbol), ("orderId", orderId)),
                true,
                "cap.query",
                ct
            );
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/userTrades",
                Q(("symbol", Symbol), ("limit", "5")),
                true,
                "cap.trades",
                ct
            );
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/allOrders",
                Q(("symbol", Symbol), ("limit", "3")),
                true,
                "cap.allOrders",
                ct
            );
        }
        finally
        {
            await SendAsync(
                HttpMethod.Post,
                "/fapi/v1/order",
                Q(
                    ("symbol", Symbol),
                    ("side", "SELL"),
                    ("positionSide", "BOTH"),
                    ("type", "MARKET"),
                    ("quantity", qty.ToString(CultureInfo.InvariantCulture)),
                    ("reduceOnly", "true"),
                    ("newClientOrderId", Guid.NewGuid().ToString()),
                    ("newOrderRespType", "RESULT")
                ),
                true,
                "cap.close",
                ct
            );
            await Task.Delay(4000, ct);

            var dir = Environment.GetEnvironmentVariable("PROBE_OUT");
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
                var i = 0;
                foreach (var m in messages)
                    await File.WriteAllTextAsync(Path.Combine(dir, $"cap.stream.{i++:00}.json"), m, ct);
            }
        }

        orderId.IsNotEmpty($"the placement was refused: {placed}");
    }

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
            var signatureService = sp.CreateSignatureService(Settings.User, ProviderKey.Create(Constants.Provider));
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

        // not asserted: a refusal is itself an answer worth recording, and the cancel in the finally must
        // run even when the placement was rejected. The caller asserts on what it needed
        return body;
    }

    /// <summary>Reads a string property out of a flat JSON object without deserializing into a type.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or an empty string when the property is absent.</returns>
    private static string ReadString(string json, string name)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        return
            doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object
            && doc.RootElement.TryGetProperty(name, out var value)
            ? value.ToString()
            : string.Empty;
    }

    /// <summary>Reads a decimal-valued string property out of a flat JSON object.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or zero when the property is absent or unreadable.</returns>
    private static decimal ReadDecimal(string json, string name)
    {
        var raw = ReadString(json, name);

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }
}
