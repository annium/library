using System;
using System.IO;
using Annium.Integrations.Social.Telegram;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Messages;
using Annium.Integrations.Social.Telegram.Integration.Receivers;
using Annium.Integrations.Social.Telegram.Internal;
using Annium.Integrations.Social.Telegram.Internal.Integration;
using Annium.Integrations.Social.Telegram.Internal.Integration.Messages;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Constants = Annium.Integrations.Social.Telegram.Internal.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddTelegramBot(this IServiceContainer container, string key)
    {
        return container.AddTelegramBot(key, sp => sp.Resolve<TelegramBotConfiguration>(), _ => { });
    }

    public static IServiceContainer AddTelegramBot(
        this IServiceContainer container,
        string key,
        Action<BotOptions> setup
    )
    {
        return container.AddTelegramBot(key, sp => sp.Resolve<TelegramBotConfiguration>(), setup);
    }

    public static IServiceContainer AddTelegramBot(
        this IServiceContainer container,
        string key,
        Func<IServiceProvider, TelegramBotConfiguration> configure
    )
    {
        return container.AddTelegramBot(key, configure, _ => { });
    }

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

    private static ApiContext CreateApiContext(IServiceProvider sp, object key)
    {
        var config = sp.ResolveKeyed<TelegramBotConfiguration>(key);

        var server = new Uri($"https://api.telegram.org/bot{config.Token}");

        var httpRequestFactory = sp.ResolveKeyed<IHttpRequestFactory>(Constants.ServiceKey);
        var serializer = sp.ResolveKeyed<ISerializer<Stream>>(Constants.SerializerKey);

        return new ApiContext(server, httpRequestFactory, serializer);
    }
}
