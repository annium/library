using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Core;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.Shared;
using Annium.Finance.Providers.Crypto.Binance.UsdFutures.Internal.User;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests.Internal.User;

/// <summary>
/// Captures the raw <c>ALGO_UPDATE</c> payloads the user data stream pushes when a conditional order is
/// placed and cancelled, so a converter for them can be written from a real message.
/// </summary>
/// <remarks>
/// <para>
/// The last documentation gap of the migration. The event's payload renders from a schema source that
/// cannot be fetched, and the earlier probes read REST only - so nothing in the tree or in the snapshot
/// says what an <c>ALGO_UPDATE</c> looks like. Guessing it would be inventing a wire format and then
/// shipping it.
/// </para>
/// <para>
/// Cheap and harmless: the conditional order's trigger sits well away from the market, so it never fires
/// and no position is ever opened. Placing and cancelling it produces the two states worth seeing. The
/// cancellation runs in a <c>finally</c>.
/// </para>
/// </remarks>
[Collection(ExchangeCollection.Name)]
[Trait(TestBlock.Name, TestBlock.Write)]
public class AlgoUpdateStreamProbeTests : ProvidersTestBase
{
    /// <summary>The symbol the probe trades, chosen for the lowest notional the venue allows.</summary>
    private const string Symbol = "DOTUSDT";

    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoUpdateStreamProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public AlgoUpdateStreamProbeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the USD-M futures provider, so the probe uses the same stream machinery production does.
    /// </summary>
    /// <param name="ctx">The fluent context to register providers into.</param>
    protected override void RegisterProvider(ProviderRegistrationContext ctx)
    {
        ctx.WithBinanceUsdFutures();
    }

    /// <summary>
    /// Opens the user data stream, places and cancels a conditional order, and saves every message seen.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = 180_000)]
    public async Task AlgoUpdate_IsCaptured()
    {
        var ct = TestContext.Current.CancellationToken;
        var sp = Get<IServiceProvider>();
        var messages = new ConcurrentQueue<string>();

        var signatureService = sp.CreateSignatureService(Settings.User, ProviderKey.Create(Constants.Provider));
        var config = BuildConfig();
        var monitor = new StatusMonitor(Logger);

        var resolver = sp.CreateListenKeyResolver(
            config,
            Endpoints.ListenKeyUriPath,
            Constants.ListenKeyKey,
            signatureService,
            monitor
        );
        using var stream = sp.CreateUserStream(config, resolver, monitor);

        stream.OnMessage += data => messages.Enqueue(Encoding.UTF8.GetString(data.Span));

        // act - give the stream a moment to come up. The gate that matters is the message wait below, so
        // this only has to be long enough for a listen key and a socket
        await Task.Delay(10_000, ct);

        var price = ReadDecimal(
            await SendAsync(HttpMethod.Get, "/fapi/v1/ticker/price", Q(("symbol", Symbol)), false, ct),
            "price"
        );
        price.IsGreater(0m, "the venue quoted no price");

        var trigger = decimal.Round(price * 1.04m, 4, MidpointRounding.ToZero);
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
            ct
        );
        var algoId = ReadString(placed, "algoId");
        algoId.IsNotEmpty($"the placement was refused: {placed}");

        try
        {
            await Expect.ToAsync(
                () => messages.Any(x => x.Contains(clientAlgoId, StringComparison.Ordinal)).IsTrue(),
                30_000
            );
        }
        finally
        {
            await SendAsync(HttpMethod.Delete, "/fapi/v1/algoOrder", Q(("algoId", algoId)), true, ct);
            // the cancellation has its own event, and it is the second state worth recording
            await Task.Delay(5000, ct);
            Save(messages);
        }
    }

    /// <summary>Writes every captured message to the probe output directory.</summary>
    /// <param name="messages">The messages seen on the stream.</param>
    private static void Save(ConcurrentQueue<string> messages)
    {
        var dir = Environment.GetEnvironmentVariable("PROBE_OUT");
        if (string.IsNullOrEmpty(dir))
            return;

        Directory.CreateDirectory(dir);
        var i = 0;
        foreach (var message in messages)
            File.WriteAllText(Path.Combine(dir, $"stream.{i++:00}.json"), message);
    }

    /// <summary>Builds a live user configuration pointing at the real endpoints.</summary>
    /// <returns>The configuration.</returns>
    private static UserConfig BuildConfig() =>
        new()
        {
            Provider = Constants.Provider,
            Key = Settings.User.Key,
            Secret = Settings.User.Secret,
            HttpApi = Endpoints.HttpApi,
            WsApi = Endpoints.WsApi,
            ListenKeyUriPath = Endpoints.UserWsUriPath,
            ListenKey = new ListenKeyConfiguration(60_000, 5_000),
            ReloadContext = new Core.Shared.Loaders.CompositeLoaderConfig(1000, 5, 5000, 1000, 100),
            ReloadOrders = new Core.Shared.Loaders.CompositeLoaderConfig(1000, 5, 5000, 1000, 100),
            ReloadTrades = new Core.Shared.Loaders.CompositeLoaderConfig(1000, 5, 5000, 1000, 100),
        };

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

    /// <summary>Sends one request to the live exchange and returns its body.</summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The endpoint path.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="signed">Whether to sign the request.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The response body.</returns>
    private async Task<string> SendAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string> parameters,
        bool signed,
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

        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>Reads a string property out of a flat JSON object.</summary>
    /// <param name="json">The payload.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or empty when absent.</returns>
    private static string ReadString(string json, string name)
    {
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(name, out var value)
            ? value.ToString()
            : string.Empty;
    }

    /// <summary>Reads a decimal-valued string property out of a flat JSON object.</summary>
    /// <param name="json">The payload.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or zero when absent.</returns>
    private static decimal ReadDecimal(string json, string name)
    {
        var raw = ReadString(json, name);

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }
}
