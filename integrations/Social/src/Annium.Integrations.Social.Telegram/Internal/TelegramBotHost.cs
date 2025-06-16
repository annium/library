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

internal sealed class TelegramBotHost : ITelegramBotHost, IAsyncDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly ITelegramApi _api;
    private readonly ITelegramMessageReceiver _receiver;
    private readonly object _key;
    private readonly IExecutor _executor;
    private readonly AsyncDisposableBox _disposable;

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

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        await _disposable.DisposeAsync();
        this.Trace("done");
    }
}
