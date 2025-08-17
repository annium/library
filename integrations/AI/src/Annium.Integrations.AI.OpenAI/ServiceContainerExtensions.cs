using System;
using System.ClientModel;
using System.Collections.Immutable;
using Annium.Core.DependencyInjection;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

namespace Annium.Integrations.AI.OpenAI;

public static class ServiceContainerExtensions
{
    private static readonly ImmutableArray<FactoryInfo> _factories =
    [
        Factory(BuildOpenAiChatClient),
        Factory(BuildOpenAiAudioClient),
    ];

    public static IServiceContainer AddOpenAi(this IServiceContainer container, OpenAiConfigs configs)
    {
        container.Add(configs).AsSelf().Singleton();

        foreach (var key in configs.Keys)
        {
            container.Add(BuildOpenAiClient).AsKeyedSelf(key).Singleton();
            foreach (var (type, factory) in _factories)
                container.Add(type, factory).AsKeyedSelf(key).Singleton();
        }

        return container;
    }

    private static OpenAIClient BuildOpenAiClient(IServiceProvider sp, object key)
    {
        var config = sp.ResolveConfig(key);
        var credential = new ApiKeyCredential(config.Key);
        var options = config.Options ?? new OpenAIClientOptions();
        var client = new OpenAIClient(credential, options);

        return client;
    }

    private static ChatClient BuildOpenAiChatClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);
        var config = sp.ResolveConfig(key);

        return client.GetChatClient(config.Model);
    }

    private static AudioClient BuildOpenAiAudioClient(IServiceProvider sp, object key)
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
    public static OpenAiConfig ResolveConfig(this IServiceProvider sp, object key)
    {
        var configs = sp.Resolve<OpenAiConfigs>();

        if (!configs.TryGetValue((string)key, out var config))
            throw new InvalidOperationException($"Requested key '{key}' was not found in configuration.");

        return config;
    }
}
