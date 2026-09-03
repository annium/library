using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Social.Telegram.Handlers;
using Annium.Social.Telegram.Integration;
using Annium.Social.Telegram.Integration.Receivers;
using Annium.Social.Telegram.Internal.Integration;
using Annium.Social.Telegram.Internal.Integration.Receivers;

namespace Annium.Social.Telegram;

/// <summary>
/// Configures the receiver, sender and handler registrations for a single keyed Telegram bot instance registered via
/// <c>AddTelegramBot</c>. A default receiver is registered from the constructor, and is replaced by whichever
/// <c>Use*</c> method is called during the setup delegate passed to <c>AddTelegramBot</c>.
/// </summary>
public sealed record BotOptions
{
    /// <summary>
    /// The DI container the receiver, sender and handler registrations are added to.
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The keyed-service key identifying this bot instance's registrations.
    /// </summary>
    private readonly string _key;

    /// <summary>
    /// Creates the options for one keyed bot instance and registers the default (polling) receiver,
    /// which any <c>Use*</c> call in the setup delegate then replaces.
    /// </summary>
    /// <param name="container">The DI container the bot's registrations are added to.</param>
    /// <param name="key">The keyed-service key identifying this bot instance.</param>
    internal BotOptions(IServiceContainer container, string key)
    {
        _container = container;
        _key = key;

        // receivers own a background poll/listen loop and are IAsyncDisposable, while the only
        // consumer (TelegramBotHost) is a singleton resolving them from the root provider — a scoped
        // registration made every receiver a captive dependency of the root scope
        _container.Add(CreateDefaultMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Singleton();
    }

    /// <summary>
    /// Registers a receiver that pulls updates via long-polling <c>getUpdates</c> calls, replacing the default
    /// receiver selected from configuration.
    /// </summary>
    public void UsePollingReceiver()
    {
        _container.Add(CreatePollingMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Singleton();
    }

    /// <summary>
    /// Registers a receiver that runs an HTTP server and receives updates pushed by Telegram to a configured
    /// webhook, replacing the default receiver selected from configuration.
    /// </summary>
    public void UseWebhookReceiver()
    {
        _container.Add(CreateWebhookMessageReceiver).AsKeyed<ITelegramMessageReceiver>(_key).Singleton();
    }

    /// <summary>
    /// Registers the given receiver instance as the update source for this bot, replacing the default receiver.
    /// </summary>
    /// <param name="receiver">The receiver instance to use.</param>
    public void UseReceiver(ITelegramMessageReceiver receiver)
    {
        _container.Add(receiver).AsKeyed<ITelegramMessageReceiver>(_key).Singleton();
    }

    /// <summary>
    /// Registers the given API client instance as the outbound sender for this bot, replacing the default client
    /// built from configuration.
    /// </summary>
    /// <param name="sender">The API client instance to use for sending messages.</param>
    public void UseSender(ITelegramApi sender)
    {
        _container.Add(sender).AsKeyed<ITelegramApi>(_key).Singleton();
    }

    /// <summary>
    /// Registers <typeparamref name="THandler"/> as the update handler for this bot; a new instance is resolved
    /// from a fresh scope for each processed update.
    /// </summary>
    /// <typeparam name="THandler">The handler type to resolve and invoke for each update.</typeparam>
    public void UseHandler<THandler>()
        where THandler : ITelegramMessageHandler
    {
        _container.Add<THandler>().AsKeyed<ITelegramMessageHandler>(_key).Scoped();
    }

    /// <summary>
    /// Registers the given handler instance as the update handler for this bot, shared as a singleton across all
    /// processed updates.
    /// </summary>
    /// <param name="handler">The handler instance to invoke for each update.</param>
    public void UseHandler(ITelegramMessageHandler handler)
    {
        _container.Add(handler).AsKeyed<ITelegramMessageHandler>(_key).Singleton();
    }

    /// <summary>
    /// Creates the default receiver for the bot's configuration: a webhook receiver when
    /// <see cref="TelegramBotConfiguration.Webhook"/> is configured, otherwise a polling receiver.
    /// </summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="key">The keyed-service key identifying this bot instance.</param>
    /// <returns>The created receiver.</returns>
    private static ITelegramMessageReceiver CreateDefaultMessageReceiver(IServiceProvider sp, object key)
    {
        var config = sp.ResolveKeyed<TelegramBotConfiguration>(key);

        return config.Webhook is null ? CreatePollingMessageReceiver(sp, key) : CreateWebhookMessageReceiver(sp, key);
    }

    /// <summary>
    /// Creates a receiver that polls Telegram's <c>getUpdates</c> endpoint on a background loop.
    /// </summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="key">The keyed-service key identifying this bot instance.</param>
    /// <returns>The created polling receiver.</returns>
    private static ITelegramMessageReceiver CreatePollingMessageReceiver(IServiceProvider sp, object key)
    {
        return new PollingMessageReceiver(sp.ResolveKeyed<ApiContext>(key), sp.Resolve<ILogger>());
    }

    /// <summary>
    /// Creates a receiver that registers a Telegram webhook and runs an HTTP server to receive pushed updates.
    /// </summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="key">The keyed-service key identifying this bot instance.</param>
    /// <returns>The created webhook receiver.</returns>
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
