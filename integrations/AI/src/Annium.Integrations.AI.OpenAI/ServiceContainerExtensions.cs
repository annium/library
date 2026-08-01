using System;
using System.ClientModel;
using System.Collections.Immutable;
using Annium.Core.DependencyInjection;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

namespace Annium.Integrations.AI.OpenAI;

// ReSharper disable InconsistentNaming
/// <summary>
/// Container extensions registering an OpenAI client and the per-capability clients derived from it.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// The capability clients built for every registered OpenAI client.
    /// </summary>
    private static readonly ImmutableArray<FactoryInfo> _factories =
    [
        Factory(BuildOpenAIChatClient),
        Factory(BuildOpenAIAudioClient),
    ];

    /// <summary>
    /// Registers an OpenAI client under <paramref name="clientId"/>, together with the chat and audio
    /// clients derived from it. Settings are resolved through <paramref name="getConfig"/> at resolution time.
    /// </summary>
    /// <param name="container">The container to register into.</param>
    /// <param name="clientId">The key the client and its derived clients are registered under.</param>
    /// <param name="getConfig">Resolves the settings of this client.</param>
    /// <returns>The container, for chaining.</returns>
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

    /// <summary>
    /// Builds the OpenAI client for a registration key from its configured credential and options.
    /// </summary>
    /// <param name="sp">The provider to resolve settings from.</param>
    /// <param name="key">The key of the client registration.</param>
    /// <returns>The configured client.</returns>
    private static OpenAIClient BuildOpenAIClient(IServiceProvider sp, object key)
    {
        var config = sp.ResolveConfig(key);
        var credential = new ApiKeyCredential(config.Key);
        var options = config.Options ?? new OpenAIClientOptions();
        var client = new OpenAIClient(credential, options);

        return client;
    }

    /// <summary>
    /// Builds the chat client for a registration key, bound to its configured model.
    /// </summary>
    /// <param name="sp">The provider to resolve settings from.</param>
    /// <param name="key">The key of the client registration.</param>
    /// <returns>The configured chat client.</returns>
    private static ChatClient BuildOpenAIChatClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);
        var config = sp.ResolveConfig(key);

        return client.GetChatClient(config.Model);
    }

    /// <summary>
    /// Builds the audio client for a registration key, bound to its configured model.
    /// </summary>
    /// <param name="sp">The provider to resolve settings from.</param>
    /// <param name="key">The key of the client registration.</param>
    /// <returns>The configured audio client.</returns>
    private static AudioClient BuildOpenAIAudioClient(IServiceProvider sp, object key)
    {
        var client = sp.ResolveKeyed<OpenAIClient>(key);
        var config = sp.ResolveConfig(key);

        return client.GetAudioClient(config.Model);
    }

    /// <summary>
    /// Pairs a capability client factory with the service type it produces.
    /// </summary>
    /// <typeparam name="T">The service type the factory produces.</typeparam>
    /// <param name="factory">The factory building the service.</param>
    /// <returns>The factory paired with its service type.</returns>
    private static FactoryInfo Factory<T>(Func<IServiceProvider, object, T> factory)
        where T : class
    {
        return new(typeof(T), factory);
    }

    /// <summary>
    /// A capability client registration: the service type and the factory producing it.
    /// </summary>
    /// <param name="Type">The service type to register.</param>
    /// <param name="Factory">The factory producing the service.</param>
    private record FactoryInfo(Type Type, Func<IServiceProvider, object, object> Factory);
}

/// <summary>
/// Helpers for resolving the settings behind a keyed OpenAI client registration.
/// </summary>
file static class ServiceProviderExtensions
{
    /// <summary>
    /// Resolves the settings registered under a client key.
    /// </summary>
    /// <param name="sp">The provider to resolve from.</param>
    /// <param name="key">The key of the client registration.</param>
    /// <returns>The settings of that client.</returns>
    public static OpenAIConfig ResolveConfig(this IServiceProvider sp, object key)
    {
        var resolveConfig = sp.ResolveKeyed<GetOpenAIConfig>(key);
        var config = resolveConfig(sp);

        return config;
    }
}
