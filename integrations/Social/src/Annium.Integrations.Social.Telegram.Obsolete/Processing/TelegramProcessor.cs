using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Obsolete.Api;
using Annium.Integrations.Social.Telegram.Obsolete.Api.Models;
using Annium.Integrations.Social.Telegram.Obsolete.Operations;
using Annium.Logging;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public class TelegramProcessor : ITelegramProcessor, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ITelegramMenuFactory _menuFactory;
    private readonly ITelegramMenuRegistry _menuRegistry;
    private readonly ITelegramProcessorRegistry _processorRegistry;
    private readonly ITelegramUserProcessorFactory _processorFactory;

    // private readonly IBotCache cache;
    private readonly ITelegramApi _api;

    public TelegramProcessor(
        ITelegramMenuFactory menuFactory,
        ITelegramMenuRegistry menuRegistry,
        ITelegramProcessorRegistry processorRegistry,
        ITelegramUserProcessorFactory processorFactory,
        // IBotCache cache,
        ITelegramApi api,
        ILogger logger
    )
    {
        _menuFactory = menuFactory;
        _menuRegistry = menuRegistry;
        _processorRegistry = processorRegistry;
        _processorFactory = processorFactory;
        _api = api;
        Logger = logger;
    }

    public async Task PollUpdatesAsync()
    {
        while (true)
        {
            var updates = await GetUpdatesAsync();
            foreach (var update in updates)
                Task.Run(() => HandleUpdateAsync(update)).GetAwaiter();
        }
    }

    private async Task<TelegramUpdate[]> GetUpdatesAsync()
    {
        try
        {
            // var offset = await cache.GetAsync<int>("lastUpdateId");
            var offset = 0;
            var updatesResult = await _api.GetUpdatesAsync(offset + 1);
            if (!updatesResult.Ok)
                return [];

            // if (updatesResult.Result.Length > 0)
            //     await cache.SetAsync("lastUpdateId", updatesResult.Result[updatesResult.Result.Length - 1].UpdateId);

            return updatesResult.Result;
        }
        catch (Exception exception)
        {
            this.Error(exception);

            return [];
        }
    }

    private async Task HandleUpdateAsync(TelegramUpdate update)
    {
        var message = update.Message;
        if (message == null || message.Chat.Type != TelegramChatType.Private)
            return;

        var userId = message.From.Id;

        var menuInProcess = _menuRegistry.HasData(userId);
        var isActive = _processorRegistry.HasData(userId);

        if (isActive)
        {
            if (_menuFactory.IsCancel(message))
                await HandleNewOperationAsync(userId);
            else
                HandleActiveOperationMessage(userId, message);
        }
        else
        {
            if (menuInProcess)
                HandleMenuMessage(userId, message);
            else
                await HandleNewOperationAsync(userId);
        }
    }

    private async Task HandleNewOperationAsync(int userId)
    {
        await _processorRegistry.GetData(userId).Item3.CancelAsync();

        var operation = await GetOperationAsync(userId);
        if (operation == null)
        {
            await _api.SendMessageAsync(userId, "Access denied");
            return;
        }

        var processor = _processorFactory.Create(userId);
        if (operation == null)
        {
            this.Error("Command {operation} is not supported", operation);
            return;
        }

        var cts = new CancellationTokenSource();

        _processorRegistry.SetData(userId, operation, processor, cts);

        try
        {
            await operation.RunAsync(userId, processor, cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            this.Error(exception);
        }
        finally
        {
            _processorRegistry.ClearData(userId);
        }
    }

    private async Task<ITelegramOperation?> GetOperationAsync(int userId)
    {
        var menu = await _menuFactory.CreateAsync(userId);

        var processor = _processorFactory.Create(userId);
        _menuRegistry.SetData(userId, menu, processor);

        try
        {
            return await menu.GetOperationAsync(userId, processor, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception exception)
        {
            this.Error(exception);
            return null;
        }
        finally
        {
            _menuRegistry.ClearData(userId);
        }
    }

    private void HandleActiveOperationMessage(int userId, TelegramMessage message)
    {
        var (_, processor, _) = _processorRegistry.GetData(userId);
        processor.HandleMessage(message);
    }

    private void HandleMenuMessage(int userId, TelegramMessage message)
    {
        _menuRegistry.GetData(userId).Item2.HandleMessage(message);
    }
}
