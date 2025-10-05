using System;
using System.ClientModel;
using System.Collections.Immutable;
using Annium.Core.DependencyInjection;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

namespace Annium.Integrations.AI.OpenAI;

// ReSharper disable InconsistentNaming
public static class ServiceContainerExtensions
{
    private static readonly ImmutableArray<FactoryInfo> _factories =
    [
        Factory(BuildOpenAIChatClient),
        Factory(BuildOpenAIAudioClient),
    ];

    public static IServiceContainer AddOpenAI(
        this IServiceContainer container,
        object clientId,
        GetOpenAIConfig getConfig
    )
    {
        container.Add(getConfig).AsKeyedSelf(clientId).Singleton();
        container.Add(BuildOpenAIClient).AsKeyedSelf(clientId).Singleton();
        foreach (var (type, factory) in _factories)
            container.Add(type, factory).AsKeyedSelf(clientId).Singleton();

        return container;
    }

    private static OpenAIClient BuildOpenAIClient(IServiceProvider sp, object key)
    {
        var config = sp.ResolveConfig(key);
        var credential = new ApiKeyCredential(config.Key);
        var options = config.Options ?? new OpenAIClientOptions();
        var client = new OpenAIClient(credential, options);

        return client;
    }

    private static ChatClient BuildOpenAIChatClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);
        var config = sp.ResolveConfig(key);

        return client.GetChatClient(config.Model);
    }

    private static AudioClient BuildOpenAIAudioClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);
        var config = sp.ResolveConfig(key);

        return client.GetAudioClient(config.Model);
    }

    private static FactoryInfo Factory<T>(Func<IServiceProvider, object, T> factory)
        where T : class
    {
        return new(typeof(T), factory);
    }

    private record FactoryInfo(Type Type, Func<IServiceProvider, object, object> Factory);
}

file static class ServiceProviderExtensions
{
    public static OpenAIConfig ResolveConfig(this IServiceProvider sp, object key)
    {
        var resolveConfig = sp.ResolveKeyed<GetOpenAIConfig>(key);
        var config = resolveConfig(sp);

        return config;
    }
}
