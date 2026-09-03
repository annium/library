using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Social.Telegram.Integration;
using Annium.Social.Telegram.Integration.Messages;
using Annium.Social.Telegram.Integration.Receivers;
using Annium.Social.Telegram.Internal;
using Annium.Social.Telegram.Internal.Integration;
using Annium.Social.Telegram.Internal.Integration.Messages;
using Constants = Annium.Social.Telegram.Internal.Constants;

namespace Annium.Social.Telegram;

/// <summary>
/// Extension methods for registering a keyed Telegram bot instance (configuration, API client, receiver and host)
/// into an <see cref="IServiceContainer"/>.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers a keyed Telegram bot under <paramref name="key"/>, resolving <see cref="TelegramBotConfiguration"/>
    /// directly from the container and using the default receiver, sender and handler wiring.
    /// </summary>
    /// <param name="container">The container to register the bot into.</param>
    /// <param name="key">The key identifying this bot instance's registrations.</param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceContainer AddTelegramBot(this IServiceContainer container, string key)
    {
        return container.AddTelegramBot(key, sp => sp.Resolve<TelegramBotConfiguration>(), _ => { });
    }

    /// <summary>
    /// Registers a keyed Telegram bot under <paramref name="key"/>, resolving <see cref="TelegramBotConfiguration"/>
    /// directly from the container and letting <paramref name="setup"/> customize the receiver, sender and handler
    /// registrations.
    /// </summary>
    /// <param name="container">The container to register the bot into.</param>
    /// <param name="key">The key identifying this bot instance's registrations.</param>
    /// <param name="setup">A callback that customizes the bot's receiver, sender and handler registrations.</param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceContainer AddTelegramBot(
        this IServiceContainer container,
        string key,
        Action<BotOptions> setup
    )
    {
        return container.AddTelegramBot(key, sp => sp.Resolve<TelegramBotConfiguration>(), setup);
    }

    /// <summary>
    /// Registers a keyed Telegram bot under <paramref name="key"/>, building <see cref="TelegramBotConfiguration"/>
    /// via <paramref name="configure"/> and using the default receiver, sender and handler wiring.
    /// </summary>
    /// <param name="container">The container to register the bot into.</param>
    /// <param name="key">The key identifying this bot instance's registrations.</param>
    /// <param name="configure">A factory that builds the bot's configuration from the service provider.</param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceContainer AddTelegramBot(
        this IServiceContainer container,
        string key,
        Func<IServiceProvider, TelegramBotConfiguration> configure
    )
    {
        return container.AddTelegramBot(key, configure, _ => { });
    }

    /// <summary>
    /// Registers a keyed Telegram bot under <paramref name="key"/>: wires up the HTTP request factory, JSON
    /// serializer, configuration, API client and host, then lets <paramref name="setup"/> customize the receiver,
    /// sender and handler registrations.
    /// </summary>
    /// <param name="container">The container to register the bot into.</param>
    /// <param name="key">The key identifying this bot instance's registrations.</param>
    /// <param name="configure">A factory that builds the bot's configuration from the service provider.</param>
    /// <param name="setup">A callback that customizes the bot's receiver, sender and handler registrations.</param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceContainer AddTelegramBot(
        this IServiceContainer container,
        string key,
        Func<IServiceProvider, TelegramBotConfiguration> configure,
        Action<BotOptions> setup
    )
    {
        container.AddHttpRequestFactory(Constants.ServiceKey);
        container.AddSerializers(Constants.ServiceKey).WithJson();

        container
            .Add<TelegramBotConfiguration>((sp, _) => configure(sp))
            .AsKeyed<TelegramBotConfiguration>(key)
            .Singleton();
        container.Add(CreateApiContext).AsKeyed<ApiContext>(key).Singleton();

        container
            .Add(static (sp, key) => new TelegramApi(sp.ResolveKeyed<IMessageApi>(key)))
            .AsKeyed<ITelegramApi>(key)
            .Singleton();
        container
            .Add(static (sp, key) => new MessageApi(sp.ResolveKeyed<ApiContext>(key), sp.Resolve<ILogger>()))
            .AsKeyed<IMessageApi>(key)
            .Singleton();
        container
            .Add(
                static (sp, key) =>
                    new TelegramBotHost(
                        sp,
                        sp.ResolveKeyed<ITelegramApi>(key),
                        sp.ResolveKeyed<ITelegramMessageReceiver>(key),
                        key,
                        sp.Resolve<ILogger>()
                    )
            )
            .AsKeyed<ITelegramBotHost>(key)
            .Singleton();

        var options = new BotOptions(container, key);
        setup(options);

        return container;
    }

    /// <summary>
    /// Builds the <see cref="ApiContext"/> for a keyed bot: the Telegram API base URL derived from its configured
    /// token, plus the shared HTTP request factory and JSON serializer.
    /// </summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="key">The key identifying this bot instance.</param>
    /// <returns>The created API context.</returns>
    private static ApiContext CreateApiContext(IServiceProvider sp, object key)
    {
        var config = sp.ResolveKeyed<TelegramBotConfiguration>(key);

        var server = new Uri($"https://api.telegram.org/bot{config.Token}");

        var httpRequestFactory = sp.ResolveKeyed<IHttpRequestFactory>(Constants.ServiceKey);
        var serializer = sp.ResolveKeyed<ISerializer<Stream>>(Constants.SerializerKey);

        return new ApiContext(server, httpRequestFactory, serializer);
    }
}
