using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Social.Telegram.Handlers;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;
using Annium.Integrations.Social.Telegram.Internal;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.Telegram.Tests;

/// <summary>
/// Tests for the bot host: how updates travel from the receiver's channel to a handler, and what shutting the
/// host down does to work already in flight.
/// </summary>
public class TelegramBotHostTests : TestBase
{
    /// <summary>
    /// The key the bot's handler is registered under.
    /// </summary>
    private const string Key = "test-bot";

    /// <summary>
    /// Creates the fixture.
    /// </summary>
    /// <param name="outputHelper">The xunit output helper test logs are written to.</param>
    public TelegramBotHostTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Each update read from the receiver reaches the handler, with the bot's own API client for replying.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RunAsync_UpdatesReceived_AreHandedToTheHandler()
    {
        // arrange
        var handler = new RecordingHandler();
        var receiver = new FakeReceiver();
        var api = new FakeApi();
        await using var host = Host(handler, receiver, api);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var run = host.RunAsync(cts.Token);

        // act
        receiver.Push(1);
        receiver.Push(2);
        await WaitUntilAsync(() => handler.Handled.Count == 2);

        // assert - both updates arrive, and the handler is handed the bot's own api rather than some other one
        var ids = handler.Handled.Select(x => x.Update.Id).OrderBy(x => x).ToArray();
        ids.Has(2).At(0).Is(1L);
        ids.At(1).Is(2L);
        handler.Handled.All(x => ReferenceEquals(x.Api, api)).IsTrue("the handler must get this bot's api");

        // cleanup
        await cts.CancelAsync();
        // VSTHRD003: `run` is this test's own task, awaited to observe how cancellation ends the loop
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await run).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Every update is handled in its own scope, so a scoped handler cannot leak state from one update into
    /// the next.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RunAsync_ScopedHandler_IsResolvedPerUpdate()
    {
        // arrange
        var instances = new ConcurrentBag<object>();
        var receiver = new FakeReceiver();
        await using var host = Host(_ => new CountingHandler(instances), receiver, new FakeApi(), scoped: true);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var run = host.RunAsync(cts.Token);

        // act
        receiver.Push(1);
        receiver.Push(2);
        await WaitUntilAsync(() => instances.Count == 2);

        // assert
        instances.Distinct().Count().Is(2, "each update must be handled in its own scope");

        // cleanup
        await cts.CancelAsync();
        // VSTHRD003: `run` is this test's own task, awaited to observe how cancellation ends the loop
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await run).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A handler that throws does not take the host down with it: the next update is still processed, and the
    /// failure is logged rather than lost.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RunAsync_HandlerThrows_KeepsProcessingAndLogs()
    {
        // arrange - the first update blows up, the second must still get through
        var handler = new RecordingHandler(failOnUpdateId: 1);
        var receiver = new FakeReceiver();
        await using var host = Host(handler, receiver, new FakeApi());
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var run = host.RunAsync(cts.Token);

        // act
        receiver.Push(1);
        receiver.Push(2);
        await WaitUntilAsync(() => handler.Handled.Any(x => x.Update.Id == 2L));

        // assert - the handler runs concurrently with the failing one, so wait for the log rather than
        // assuming it landed before update 2 finished
        await WaitUntilAsync(() => Logs.Any(x => x.Level >= LogLevel.Error));

        // cleanup
        await cts.CancelAsync();
        // VSTHRD003: `run` is this test's own task, awaited to observe how cancellation ends the loop
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await run).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A completed update channel ends the run: the receiver stopping is how the host learns to stop too.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RunAsync_ReceiverCompletes_Returns()
    {
        // arrange
        var receiver = new FakeReceiver();
        await using var host = Host(new RecordingHandler(), receiver, new FakeApi());
        var run = host.RunAsync(TestContext.Current.CancellationToken);

        // act
        receiver.Complete();

        // assert - without this the host would wait forever on a receiver that has already stopped
        // VSTHRD003: `run` is this test's own task
#pragma warning disable VSTHRD003
        await run;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A receiver that fails takes the run down with it. Both receivers complete their channel with the
    /// error precisely so the host surfaces it instead of waiting forever for updates that cannot come.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RunAsync_ReceiverFaults_PropagatesTheFailure()
    {
        // arrange
        var receiver = new FakeReceiver();
        await using var host = Host(new RecordingHandler(), receiver, new FakeApi());
        var run = host.RunAsync(TestContext.Current.CancellationToken);

        // act
        receiver.Fault(new InvalidOperationException("webhook could not be registered"));

        // assert - swallowing this would leave the caller believing the bot is running
        // VSTHRD003: `run` is this test's own task
#pragma warning disable VSTHRD003
        var error = await Wrap.It(async () => await run).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
        error.Message.Is("webhook could not be registered");
    }

    /// <summary>
    /// A host that was never started disposes cleanly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_NeverStarted_DoesNotThrow()
    {
        // arrange
        var host = Host(new RecordingHandler(), new FakeReceiver(), new FakeApi());

        // act & assert
        await host.DisposeAsync();
    }

    /// <summary>
    /// Disposing while a handler is still running waits for it instead of abandoning it mid-flight.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_WhileHandlerRunning_WaitsForIt()
    {
        // arrange
        var release = new TaskCompletionSource();
        var entered = new TaskCompletionSource();
        var handler = new BlockingHandler(entered, release);
        var receiver = new FakeReceiver();
        var host = Host(handler, receiver, new FakeApi());
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var run = host.RunAsync(cts.Token);

        receiver.Push(1);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        // act - dispose while the handler sits inside ProcessAsync
        var dispose = host.DisposeAsync();
        release.SetResult();
        await dispose;

        // assert
        handler.Completed.IsTrue("disposal must let in-flight handling finish");

        // cleanup
        await cts.CancelAsync();
        // VSTHRD003: `run` is this test's own task, awaited to observe how cancellation ends the loop
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await run).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Builds a host wired to the given doubles, with the handler registered as a singleton.
    /// </summary>
    /// <param name="handler">The handler every update is routed to.</param>
    /// <param name="receiver">The receiver supplying updates.</param>
    /// <param name="api">The api handed to the handler.</param>
    /// <returns>The host under test.</returns>
    private TelegramBotHost Host(ITelegramMessageHandler handler, FakeReceiver receiver, FakeApi api) =>
        Host(_ => handler, receiver, api, scoped: false);

    /// <summary>
    /// Builds a host wired to the given doubles.
    /// </summary>
    /// <param name="handler">Factory producing the handler for a scope.</param>
    /// <param name="receiver">The receiver supplying updates.</param>
    /// <param name="api">The api handed to the handler.</param>
    /// <param name="scoped">Whether the handler is registered per scope rather than as a singleton.</param>
    /// <returns>The host under test.</returns>
    private TelegramBotHost Host(
        Func<IServiceProvider, ITelegramMessageHandler> handler,
        FakeReceiver receiver,
        FakeApi api,
        bool scoped
    )
    {
        var container = new ServiceContainer();
        var registration = container.Add(handler).AsKeyed<ITelegramMessageHandler>(Key);
        if (scoped)
            registration.Scoped();
        else
            registration.Singleton();

        var provider = container.BuildServiceProvider();

        return new TelegramBotHost(provider, api, receiver, Key, Logger);
    }

    /// <summary>
    /// Polls until the condition holds, failing the test if it does not within 10 seconds.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var sw = Stopwatch.StartNew();
        while (!condition() && sw.Elapsed < TimeSpan.FromSeconds(10))
            await Task.Delay(25, TestContext.Current.CancellationToken);

        condition().IsTrue("condition was not met within 10s");
    }

    /// <summary>
    /// A receiver whose channel the test writes to directly, standing in for polling or a webhook.
    /// </summary>
    private sealed class FakeReceiver : ITelegramMessageReceiver
    {
        /// <summary>
        /// Gets the channel reader the host consumes.
        /// </summary>
        public ChannelReader<Update> Updates => _channel.Reader;

        /// <summary>
        /// The channel updates are pushed through.
        /// </summary>
        private readonly Channel<Update> _channel = Channel.CreateUnbounded<Update>();

        /// <summary>
        /// Publishes an update with the given id.
        /// </summary>
        /// <param name="id">The update id.</param>
        public void Push(long id) => _channel.Writer.TryWrite(new Update { Id = id });

        /// <summary>
        /// Completes the channel, as a stopping receiver does.
        /// </summary>
        public void Complete() => _channel.Writer.TryComplete();

        /// <summary>
        /// Completes the channel with a failure, as a receiver that could not start does.
        /// </summary>
        /// <param name="error">The failure to surface to the consumer.</param>
        public void Fault(Exception error) => _channel.Writer.TryComplete(error);
    }

    /// <summary>
    /// An api the host hands to handlers; the tests only check identity, never call through it.
    /// </summary>
    private sealed class FakeApi : ITelegramApi
    {
        /// <summary>
        /// Gets the message API, unused by these tests.
        /// </summary>
        public Integration.Messages.IMessageApi Messages => throw new NotSupportedException();
    }

    /// <summary>
    /// Records what it was handed, and optionally fails on one specific update.
    /// </summary>
    private sealed class RecordingHandler : ITelegramMessageHandler
    {
        /// <summary>
        /// The updates handled so far, with the api each was handed.
        /// </summary>
        public ConcurrentBag<(Update Update, ITelegramApi Api)> Handled { get; } = [];

        /// <summary>
        /// The update id to throw on, if any.
        /// </summary>
        private readonly long? _failOnUpdateId;

        /// <summary>
        /// Creates the handler.
        /// </summary>
        /// <param name="failOnUpdateId">The update id to throw on, or null to handle everything.</param>
        public RecordingHandler(long? failOnUpdateId = null)
        {
            _failOnUpdateId = failOnUpdateId;
        }

        /// <summary>
        /// Records the update, throwing first if it is the one this handler is set to fail on.
        /// </summary>
        /// <param name="update">The update to process.</param>
        /// <param name="api">The API client for replying.</param>
        /// <returns>A completed task.</returns>
        public Task ProcessAsync(Update update, ITelegramApi api)
        {
            if (update.Id == _failOnUpdateId)
                throw new InvalidOperationException($"handler failed on update {update.Id}");

            Handled.Add((update, api));

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Records its own instance, so the test can count how many were created.
    /// </summary>
    private sealed class CountingHandler : ITelegramMessageHandler
    {
        /// <summary>
        /// The bag every created instance adds itself to when it handles an update.
        /// </summary>
        private readonly ConcurrentBag<object> _instances;

        /// <summary>
        /// Creates the handler.
        /// </summary>
        /// <param name="instances">The bag to record this instance in.</param>
        public CountingHandler(ConcurrentBag<object> instances)
        {
            _instances = instances;
        }

        /// <summary>
        /// Records this instance.
        /// </summary>
        /// <param name="update">The update to process.</param>
        /// <param name="api">The API client for replying.</param>
        /// <returns>A completed task.</returns>
        public Task ProcessAsync(Update update, ITelegramApi api)
        {
            _instances.Add(this);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Blocks inside handling until released, so disposal can be observed while work is in flight.
    /// </summary>
    private sealed class BlockingHandler : ITelegramMessageHandler
    {
        /// <summary>
        /// Whether handling ran to completion.
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Signalled once handling has started.
        /// </summary>
        private readonly TaskCompletionSource _entered;

        /// <summary>
        /// Awaited inside handling; completing it lets handling finish.
        /// </summary>
        private readonly TaskCompletionSource _release;

        /// <summary>
        /// Creates the handler.
        /// </summary>
        /// <param name="entered">Signalled once handling has started.</param>
        /// <param name="release">Awaited inside handling.</param>
        public BlockingHandler(TaskCompletionSource entered, TaskCompletionSource release)
        {
            _entered = entered;
            _release = release;
        }

        /// <summary>
        /// Signals that handling started, waits to be released, then records completion.
        /// </summary>
        /// <param name="update">The update to process.</param>
        /// <param name="api">The API client for replying.</param>
        /// <returns>A task that completes once released.</returns>
        public async Task ProcessAsync(Update update, ITelegramApi api)
        {
            _entered.TrySetResult();
            // VSTHRD003: waiting on the test's release signal is the whole point of this double
#pragma warning disable VSTHRD003
            await _release.Task;
#pragma warning restore VSTHRD003
            Completed = true;
        }
    }
}
