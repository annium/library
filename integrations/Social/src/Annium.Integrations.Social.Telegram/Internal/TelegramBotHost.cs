using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Execution.Background;
using Annium.Integrations.Social.Telegram.Handlers;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Integrations.Social.Telegram.Internal;

/// <summary>
/// Default <see cref="ITelegramBotHost"/> implementation: starts the receiver, then dispatches each received update
/// to a handler resolved from a fresh DI scope, running handlers concurrently via an <see cref="IExecutor"/>.
/// </summary>
internal sealed class TelegramBotHost : ITelegramBotHost, IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// The logger used to record lifecycle events for this bot host.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The service provider used to create a scope per processed update.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The API client passed to the handler for each processed update.
    /// </summary>
    private readonly ITelegramApi _api;

    /// <summary>
    /// The receiver whose update channel this host reads from.
    /// </summary>
    private readonly ITelegramMessageReceiver _receiver;

    /// <summary>
    /// The keyed-service key used to resolve the handler for this bot instance.
    /// </summary>
    private readonly object _key;

    /// <summary>
    /// Runs update-handling callbacks concurrently.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Aggregates disposables owned by this host, including the executor.
    /// </summary>
    private readonly AsyncDisposableBox _disposable;

    /// <summary>
    /// Creates the host for one keyed bot instance.
    /// </summary>
    /// <param name="sp">The provider used to resolve the bot's keyed message handlers.</param>
    /// <param name="api">The API the handlers reply through.</param>
    /// <param name="receiver">The receiver supplying incoming updates.</param>
    /// <param name="key">The keyed-service key identifying this bot instance.</param>
    /// <param name="logger">The logger used to trace the host lifecycle.</param>
    public TelegramBotHost(
        IServiceProvider sp,
        ITelegramApi api,
        ITelegramMessageReceiver receiver,
        object key,
        ILogger logger
    )
    {
        _sp = sp;
        _api = api;
        _receiver = receiver;
        _key = key;
        Logger = logger;

        _disposable = Disposable.AsyncBox(logger);
        _disposable += _executor = Executor.Concurrent<TelegramBotHost>(logger);
    }

    /// <summary>
    /// Starts the executor, then reads updates from the receiver's channel until <paramref name="ct"/> is canceled
    /// or the channel completes, scheduling each update to be processed by a handler resolved from a new DI scope.
    /// </summary>
    /// <param name="ct">The token used to stop processing.</param>
    /// <returns>A task that completes once update processing has stopped.</returns>
    public async Task RunAsync(CancellationToken ct)
    {
        this.Trace("start");

        _executor.Start(ct);

        this.Trace("started, handle messages");
        while (await _receiver.Updates.WaitToReadAsync(ct))
        while (_receiver.Updates.TryRead(out var update))
        {
            _executor.Schedule(async () =>
            {
                await using var scope = _sp.CreateAsyncScope();
                var handler = scope.ServiceProvider.ResolveKeyed<ITelegramMessageHandler>(_key);
                await handler.ProcessAsync(update, _api);
            });
        }

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the executor and any other resources owned by this host.
    /// </summary>
    /// <returns>A task that completes once disposal has finished.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        await _disposable.DisposeAsync();
        this.Trace("done");
    }
}
