using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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
/// Lets one real conditional order trigger, and records what the exchange does with it afterwards: which
/// store holds it, what <c>algoStatus</c> it settles on, and whether the order it became appears among
/// ordinary orders.
/// </summary>
/// <remarks>
/// <para>
/// This answers the one question the migration cannot be designed around. Binance publishes seven algo
/// statuses against our six order statuses, and <c>FINISHED</c> is documented as "filled <em>or</em>
/// canceled" - so it maps to nothing on its own. If a triggered conditional order produces an ordinary
/// order, the ordinary order carries the outcome and the algo record needs no terminal mapping at all;
/// if it does not, the algo record is the only source and the ambiguity has to be resolved some other way.
/// </para>
/// <para>
/// Costs a real position. The trigger sits 0.08% above the market: a conditional order that would fire at
/// once is refused with <c>-2021 Order would immediately trigger</c> - the first thing this probe
/// established - so the trigger has to be reached by ordinary movement rather than manufactured, and the
/// probe waits up to four minutes for it. The quantity is the venue's minimum notional, the position is
/// closed with a reduce-only market order in a <c>finally</c> along with cancelling the conditional order
/// if it never triggered, and the account is asserted flat at the end rather than assumed to be.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Write)]
public class AlgoOrderTriggerProbeTests : ProvidersTestBase
{
    /// <summary>The symbol the probe trades, chosen for the lowest notional the venue allows.</summary>
    private const string Symbol = "DOTUSDT";

    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoOrderTriggerProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoOrderTriggerProbeTests(ITestOutputHelper outputHelper)
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
    /// Places a conditional order just above the market, waits for it to trigger, records every store
    /// afterwards, and leaves the account flat.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 300_000)]
    public async Task TriggeredAlgoOrder_RecordsWhereItEndsUp()
    {
        var ct = TestContext.Current.CancellationToken;

        var price = ReadDecimal(
            await SendAsync(HttpMethod.Get, "/fapi/v1/ticker/price", Q(("symbol", Symbol)), false, "ticker", ct),
            "price"
        );
        price.IsGreater(0m, "the venue quoted no price, so the probe cannot size an order");

        // a BUY stop triggers on a rise, so the trigger must sit above the market - and not by a hair:
        // a conditional order that would fire at once is refused outright with -2021, which is itself the
        // first thing this probe established. 0.08% is above that floor and inside a minute or two of
        // ordinary movement on a liquid symbol, so the trigger is reached rather than manufactured
        var trigger = decimal.Round(price * 1.0008m, 4, MidpointRounding.ToZero);
        var qty = decimal.Ceiling(5.5m / price * 10m) / 10m;
        var clientAlgoId = Guid.NewGuid().ToString();

        var placed = await SendAsync(
            HttpMethod.Post,
            "/fapi/v1/algoOrder",
            Q(
                ("algoType", "CONDITIONAL"),
                ("symbol", Symbol),
                ("side", "BUY"),
                ("positionSide", "BOTH"),
                ("type", "STOP_MARKET"),
                ("quantity", qty.ToString(CultureInfo.InvariantCulture)),
                ("triggerPrice", trigger.ToString(CultureInfo.InvariantCulture)),
                ("clientAlgoId", clientAlgoId),
                ("newOrderRespType", "RESULT")
            ),
            true,
            "trigger.place",
            ct
        );
        var algoId = ReadString(placed, "algoId");
        algoId.IsNotEmpty($"the placement was refused: {placed}");

        var triggered = false;
        try
        {
            // poll the algo store until it leaves NEW - that transition is the event being measured
            for (var i = 0; i < 80 && !triggered; i++)
            {
                await Task.Delay(3000, ct);
                var open = await SendAsync(
                    HttpMethod.Get,
                    "/fapi/v1/allAlgoOrders",
                    Q(("symbol", Symbol)),
                    true,
                    "trigger.poll",
                    ct
                );
                triggered =
                    open.Contains(clientAlgoId, StringComparison.Ordinal)
                    && !open.Contains("\"algoStatus\":\"NEW\"", StringComparison.Ordinal);
            }

            // whatever happened, record every store: the algo history, the open algo orders, and the
            // ordinary orders - the last is the one the design question turns on
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/allAlgoOrders",
                Q(("symbol", Symbol)),
                true,
                "trigger.allAlgoOrders",
                ct
            );
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/openAlgoOrders",
                Q(("symbol", Symbol)),
                true,
                "trigger.openAlgoOrders",
                ct
            );
            await SendAsync(
                HttpMethod.Get,
                "/fapi/v1/allOrders",
                Q(("symbol", Symbol), ("limit", "10")),
                true,
                "trigger.allOrders",
                ct
            );
        }
        finally
        {
            await CleanUpAsync(algoId, qty, ct);
        }

        triggered.IsTrue(
            "the conditional order never left NEW within four minutes - the market did not reach the trigger, so nothing about triggering was measured and the run says nothing either way"
        );
    }

    /// <summary>Cancels the conditional order if it survives, closes any position, and asserts the account is flat.</summary>
    /// <param name="algoId">The conditional order to cancel.</param>
    /// <param name="qty">The quantity that would have been opened.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanUpAsync(string algoId, decimal qty, CancellationToken ct)
    {
        await SendAsync(HttpMethod.Delete, "/fapi/v1/algoOrder", Q(("algoId", algoId)), true, "cleanup.cancelAlgo", ct);

        // reduce-only, so it can only ever close: if the order never triggered there is nothing to close and
        // the exchange refuses it, which is the outcome we want rather than an opposite position
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
            "cleanup.close",
            ct
        );

        var account = await SendAsync(HttpMethod.Get, "/fapi/v2/account", Q(), true, "cleanup.account", ct);
        using var doc = JsonDocument.Parse(account);
        var open = 0;
        if (doc.RootElement.TryGetProperty("positions", out var positions))
            foreach (var p in positions.EnumerateArray())
                if (
                    p.TryGetProperty("positionAmt", out var amt)
                    && decimal.TryParse(amt.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
                    && v != 0m
                )
                    open++;

        open.Is(0, "the probe left a position open, which is the one outcome cleanup exists to prevent");
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

        return body;
    }

    /// <summary>Reads a string property out of a flat JSON object.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or an empty string when absent.</returns>
    private static string ReadString(string json, string name)
    {
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(name, out var value)
            ? value.ToString()
            : string.Empty;
    }

    /// <summary>Reads a decimal-valued string property out of a flat JSON object.</summary>
    /// <param name="json">The payload to read.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or zero when absent or unreadable.</returns>
    private static decimal ReadDecimal(string json, string name)
    {
        var raw = ReadString(json, name);

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }
}
