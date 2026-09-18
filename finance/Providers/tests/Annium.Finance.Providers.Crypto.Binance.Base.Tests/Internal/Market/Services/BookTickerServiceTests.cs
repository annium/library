using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Connectors.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Market;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Crypto.Binance.Base.Internal.Market.Services;
using Annium.Finance.Providers.Crypto.Binance.Base.Market;
using Annium.Finance.Providers.Crypto.Binance.Base.Market.Contracts.Converters;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Converters;
using Annium.Finance.Providers.Tests.Lib;
using Annium.Finance.Providers.Tests.Lib.Infrastructure;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Tests.Internal.Market.Services;

/// <summary>
/// Pins the connection lifecycle of <see cref="BookTickerService"/> - what it sends when a topic is added or
/// removed, what it sends again after the connection drops, and what it does with a payload - against a local
/// WebSocket server rather than against Binance.
/// </summary>
/// <remarks>
/// None of this was reachable before: the service was executed only by live tests that asserted a ticker had
/// arrived, which says nothing about resubscribe, unsubscribe or a payload that does not parse. The
/// resubscribe case is the one worth the fixture on its own - a connector that silently fails to resubscribe
/// looks exactly like a connected one, and delivers nothing.
/// </remarks>
public class BookTickerServiceTests : ProvidersTestBase
{
    /// <summary>
    /// How long any test here may run before xUnit fails it, in milliseconds.
    /// </summary>
    /// <remarks>
    /// Everything waited on is local - a socket on loopback, a frame already sent - so a wait that outlives
    /// this is not slow, it is stuck. Without the deadline it would hang the run instead of failing it.
    /// </remarks>
    private const int TimeoutMs = 30_000;

    /// <summary>
    /// The key the ticker serializer is registered and resolved under.
    /// </summary>
    private const string SerializerKey = "book-ticker";

    /// <summary>
    /// Initializes a new instance of the <see cref="BookTickerServiceTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper to route trace logging to.</param>
    public BookTickerServiceTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
            container
                .AddSerializers(SerializerKey)
                .WithJson(
                    new JsonSerializerOptions()
                        .ResetConverters()
                        .AddConverter<InstrumentTickerConverter>()
                        .AddConverter<StreamDataConverter<InstrumentTicker?>>(),
                    true
                )
        );
    }

    /// <summary>
    /// Subscribing sends one SUBSCRIBE frame naming the topic of every symbol given.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Subscribe_SendsOneFrameWithEveryTopic()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        service.Subscribe(["BTCUSDT", "ETHUSDT"]);

        // assert
        var request = await ReadRequestAsync(connection, ct);
        request.Method.Is("SUBSCRIBE");
        request.Params.IsEqual(new[] { "btcusdt@bookTicker", "ethusdt@bookTicker" });
    }

    /// <summary>
    /// A second subscribe asks only for the topics not already tracked.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Subscribe_SendsOnlyNewTopics()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        service.Subscribe(["BTCUSDT"]);
        await ReadRequestAsync(connection, ct);

        // act
        service.Subscribe(["BTCUSDT", "ETHUSDT"]);

        // assert
        var request = await ReadRequestAsync(connection, ct);
        request.Method.Is("SUBSCRIBE");
        request.Params.IsEqual(new[] { "ethusdt@bookTicker" });
    }

    /// <summary>
    /// Unsubscribing sends the frame and stops tracking the topic, so subscribing again asks for it anew.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Unsubscribe_SendsFrameAndStopsTracking()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        service.Subscribe(["BTCUSDT"]);
        await ReadRequestAsync(connection, ct);

        // act
        service.Unsubscribe(["BTCUSDT"]);

        // assert
        var unsubscribe = await ReadRequestAsync(connection, ct);
        unsubscribe.Method.Is("UNSUBSCRIBE");
        unsubscribe.Params.IsEqual(new[] { "btcusdt@bookTicker" });

        // the topic is tracked no longer, so the same symbol is a new one again
        service.Subscribe(["BTCUSDT"]);
        var resubscribe = await ReadRequestAsync(connection, ct);
        resubscribe.Method.Is("SUBSCRIBE");
        resubscribe.Params.IsEqual(new[] { "btcusdt@bookTicker" });
    }

    /// <summary>
    /// A connection dropped by the server is reconnected, and every tracked topic is subscribed to again.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Reconnect_ResubscribesEveryTrackedTopic()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        service.Subscribe(["BTCUSDT", "ETHUSDT"]);
        await ReadRequestAsync(connection, ct);

        // act
        connection.Drop();

        // assert
        var reconnected = await server.WaitConnectionAsync(ct);
        var request = await ReadRequestAsync(reconnected, ct);
        request.Method.Is("SUBSCRIBE");
        request.Params.IsEqual(new[] { "btcusdt@bookTicker", "ethusdt@bookTicker" });
    }

    /// <summary>
    /// A book ticker payload is raised as an <see cref="InstrumentTicker"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Data_IsRaised()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        var tickers = Listen(service);

        // act
        await connection.SendAsync(Payload("btcusdt@bookTicker", "BTCUSDT", "1.5", "2.5"), ct);

        // assert
        var ticker = await tickers.Reader.ReadAsync(ct);
        ticker.Is(new InstrumentTicker("BTCUSDT", 1.5m, 2.5m));
    }

    /// <summary>
    /// A payload that does not parse is dropped, and the stream goes on delivering the ones that do.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Data_UnparsableIsDroppedAndStreamSurvives()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);
        var tickers = Listen(service);

        // act
        // an envelope with no data at all, and one whose ticker is priceless - the two ways a message is
        // bypassed, and neither may take the stream down with it
        await connection.SendAsync("""{"stream":"btcusdt@bookTicker"}""", ct);
        await connection.SendAsync(Payload("btcusdt@bookTicker", "BTCUSDT", "0", "0"), ct);
        await connection.SendAsync(Payload("btcusdt@bookTicker", "BTCUSDT", "1.5", "2.5"), ct);

        // assert
        var ticker = await tickers.Reader.ReadAsync(ct);
        ticker.Is(new InstrumentTicker("BTCUSDT", 1.5m, 2.5m));
    }

    /// <summary>
    /// A dropped connection puts the connector back into connecting, so a consumer sees the gap.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Drop_ReportsConnectingAgain()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        var connecting = monitor.WaitStatusAsync(ConnectorStatus.Connecting, ct);
        connection.Drop();

        // assert
        await connecting;
    }

    /// <summary>
    /// Disposing closes the connection and unregisters the service from the monitor, rather than leaving it
    /// there as a disconnected target.
    /// </summary>
    /// <remarks>
    /// The failure this pins is not an error but a state nothing recovers from: a disposed service left
    /// registered drags the aggregate down forever, and the connector it belongs to can never report itself
    /// connected again for as long as it lives.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Dispose_ClosesConnectionAndLeavesTheMonitor()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());

        // a second target, so that the monitor still has one after the service leaves - with none at all
        // it reports disconnected by definition, and the assertion would hold for the wrong reason
        var peer = monitor.CreateReporter();
        peer.Bind(new object(), ConnectorStatus.Connected);

        var service = CreateService(server, monitor);
        var connection = await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        // act
        // VSTHRD103: the service's teardown is synchronous - the warning fires only because a DisposeAsync
        // extension exists for IDisposable - and disposing it is the act under test
#pragma warning disable VSTHRD103
        service.Dispose();
#pragma warning restore VSTHRD103

        // assert
        await connection.WhenClosed.WaitAsync(ct);
        monitor.Status.Is(ConnectorStatus.Connected);
    }

    /// <summary>
    /// A control frame that cannot go out is reported on the connector's error channel.
    /// </summary>
    /// <remarks>
    /// The send is fire-and-forget by necessity - subscribing is synchronous, and on reconnect it runs on
    /// the socket's thread - so the error channel is the only place its failure can surface. Dropped, a
    /// subscribe that never reached the exchange left a connector that looked subscribed and delivered
    /// nothing.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact(Timeout = TimeoutMs)]
    public async Task Subscribe_FailedSendReachesTheErrorChannel()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        var server = this.RunWebSocketServer();
        var monitor = new StatusMonitor(Get<ILogger>());
        using var service = CreateService(server, monitor);
        await server.WaitConnectionAsync(ct);
        await monitor.WaitStatusAsync(ConnectorStatus.Connected, ct);

        var errors = Channel.CreateUnbounded<ConnectorError>();
        monitor.OnError += error => errors.Writer.TryWrite(error);

        // the server goes away for good, so the client cannot reconnect and the send has nowhere to go -
        // dropping one connection would not do, since the client reconnects immediately and the send
        // would race the new socket
        await server.DisposeAsync();
        await monitor.WaitStatusAsync(ConnectorStatus.Connecting, ct);

        // act
        service.Subscribe(["BTCUSDT"]);

        // assert
        // read until the send failure, rather than asserting on the first error: the socket keeps trying
        // to reconnect to a server that is gone, and each refusal is an error of its own on the same
        // channel. Taking the first one made this test depend on which of the two lost the race
        var error = await ReadErrorAsync(errors, x => x.Message.Contains("SUBSCRIBE"), ct);
        error.Message.Contains("failed to send").IsTrue();
    }

    /// <summary>
    /// Reads errors until one matches, so errors raised for other reasons do not decide the assertion.
    /// </summary>
    /// <param name="errors">The channel errors are written into.</param>
    /// <param name="match">What the awaited error looks like.</param>
    /// <param name="ct">The test's cancellation token, so an error that never arrives ends with the test.</param>
    /// <returns>The first matching error.</returns>
    private static async Task<ConnectorError> ReadErrorAsync(
        Channel<ConnectorError> errors,
        Func<ConnectorError, bool> match,
        CancellationToken ct
    )
    {
        while (true)
        {
            var error = await errors.Reader.ReadAsync(ct);
            if (match(error))
                return error;
        }
    }

    /// <summary>
    /// Builds a service pointed at the given local server.
    /// </summary>
    /// <param name="server">The local server the service connects to.</param>
    /// <param name="monitor">The monitor the service reports its connection status into.</param>
    /// <returns>The service under test.</returns>
    private BookTickerService CreateService(TestWebSocketServer server, StatusMonitor monitor)
    {
        var config = new TestMarketConfig
        {
            Provider = "test",
            HttpApi = new Uri("http://localhost"),
            WsApi = server.Uri,
            WsUriPath = "/",
        };

        return new BookTickerService(
            config,
            this.GetJsonSerializer(SerializerKey),
            monitor.CreateReporter(),
            Get<ILogger>()
        );
    }

    /// <summary>
    /// Collects every ticker the service raises, so a test can await the next one.
    /// </summary>
    /// <param name="service">The service to listen to.</param>
    /// <returns>The channel the tickers are written into.</returns>
    private static Channel<InstrumentTicker> Listen(BookTickerService service)
    {
        var tickers = Channel.CreateUnbounded<InstrumentTicker>();
        service.OnData += ticker => tickers.Writer.TryWrite(ticker);

        return tickers;
    }

    /// <summary>
    /// Reads the next control frame the service sent, parsed into its method and topics.
    /// </summary>
    /// <param name="connection">The connection the service is sending on.</param>
    /// <param name="ct">The test's cancellation token.</param>
    /// <returns>The parsed control request.</returns>
    private static async Task<ControlRequest> ReadRequestAsync(TestWebSocketConnection connection, CancellationToken ct)
    {
        var raw = await connection.WaitMessageAsync(ct);

        return JsonSerializer.Deserialize<ControlRequest>(raw, JsonSerializerOptions.Web).NotNull();
    }

    /// <summary>
    /// Builds a combined-stream book ticker payload, as Binance sends it.
    /// </summary>
    /// <param name="stream">The stream name the envelope carries.</param>
    /// <param name="symbol">The instrument symbol.</param>
    /// <param name="bid">The bid price, as the string Binance sends.</param>
    /// <param name="ask">The ask price, as the string Binance sends.</param>
    /// <returns>The payload text.</returns>
    private static string Payload(string stream, string symbol, string bid, string ask) =>
        $$$"""{"stream":"{{{stream}}}","data":{"s":"{{{symbol}}}","b":"{{{bid}}}","a":"{{{ask}}}"}}""";

    /// <summary>
    /// The SUBSCRIBE/UNSUBSCRIBE frame the service sends, as the server sees it.
    /// </summary>
    /// <param name="Method">The control method.</param>
    /// <param name="Params">The topics the request applies to.</param>
    private sealed record ControlRequest(string Method, IReadOnlyList<string> Params);
}

/// <summary>
/// A market configuration pointed at a local server.
/// </summary>
file sealed record TestMarketConfig : MarketConfigBase;
