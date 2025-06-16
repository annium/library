using System;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Social.Telegram.Handlers;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Integrations.Social.Telegram.Internal.Integration;
using Annium.Integrations.Social.Telegram.Internal.Integration.Receivers;
using Annium.Logging;

namespace Annium.Integrations.Social.Telegram;

public sealed record BotOptions
{
    private readonly IServiceContainer _container;
    private readonly string _key;

    internal BotOptions(IServiceContainer container, string key)
    {
        _container = container;
        _key = key;

        _container.Add(CreateDefaultMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Scoped();
    }

    public void UsePollingReceiver()
    {
        _container.Add(CreatePollingMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Scoped();
    }

    public void UseWebhookReceiver()
    {
        _container.Add(CreateWebhookMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Scoped();
    }

    public void UseReceiver(ITelegramMessageReceiver receiver)
    {
        _container.Add(receiver).AsKeyed<ITelegramMessageReceiver>(_key).Singleton();
    }

    public void UseSender(ITelegramApi sender)
    {
        _container.Add(sender).AsKeyed<ITelegramApi>(_key).Singleton();
    }

    public void UseHandler<THandler>()
        where THandler : ITelegramMessageHandler
    {
        _container.Add<THandler>().AsKeyed<ITelegramMessageHandler>(_key).Scoped();
    }

    public void UseHandler(ITelegramMessageHandler handler)
    {
        _container.Add(handler).AsKeyed<ITelegramMessageHandler>(_key).Singleton();
    }

    private static ITelegramMessageReceiver CreateDefaultMessageReceiver(IServiceProvider sp, object key)
    {
        var config = sp.ResolveKeyed<TelegramBotConfiguration>(key);

        return config.Webhook is null ? CreatePollingMessageReceiver(sp, key) : CreateWebhookMessageReceiver(sp, key);
    }

    private static ITelegramMessageReceiver CreatePollingMessageReceiver(IServiceProvider sp, object key)
    {
        return new PollingMessageReceiver(sp.ResolveKeyed<ApiContext>(key), sp.Resolve<ILogger>());
    }

    private static ITelegramMessageReceiver CreateWebhookMessageReceiver(IServiceProvider sp, object key)
    {
        return new WebhookMessageReceiver(
            sp,
            sp.ResolveKeyed<TelegramBotConfiguration>(key),
            sp.ResolveKeyed<ApiContext>(key),
            sp.Resolve<ILogger>()
        );
    }
}
