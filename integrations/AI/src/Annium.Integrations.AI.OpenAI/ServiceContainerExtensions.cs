using System;
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
        var configs = sp.Resolve<OpenAiConfigs>();

        return configs.TryGetValue((string)key, out var keyData)
            ? new OpenAIClient(keyData.Key)
            : throw new InvalidOperationException($"Requested key '{key}' was not found in configuration.");
    }

    private static ChatClient BuildOpenAiChatClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);

        return client.GetChatClient("gpt-4o");
    }

    private static AudioClient BuildOpenAiAudioClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);

        return client.GetAudioClient("whisper-1");
    }

    private static FactoryInfo Factory<T>(Func<IServiceProvider, object, T> factory)
        where T : class
    {
        return new(typeof(T), factory);
    }

    private record FactoryInfo(Type Type, Func<IServiceProvider, object, object> Factory);
}
