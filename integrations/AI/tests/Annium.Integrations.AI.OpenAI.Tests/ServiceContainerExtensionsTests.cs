using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using Xunit;

namespace Annium.Integrations.AI.OpenAI.Tests;

/// <summary>
/// Tests that OpenAI clients resolve under the key they were registered with.
/// </summary>
public class ServiceContainerExtensionsTests
{
    /// <summary>
    /// Chat and audio clients resolve independently for each registered client key, each bound to the
    /// model its own configuration names — the contract the Build*Client factories document.
    /// </summary>
    [Fact]
    public void RegisterConfiguration_Resolve_BindsEachClientToItsConfiguredModel()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddOpenAI("chat-client", _ => new OpenAIConfig("chatkey", "gpt-5", null));
        container.AddOpenAI("audio-client", _ => new OpenAIConfig("audiokey", "whisper-1", null));

        var provider = container.BuildServiceProvider();

        // act
        var chat = provider.ResolveKeyed<ChatClient>("chat-client");
        var audio = provider.ResolveKeyed<AudioClient>("audio-client");

        // assert - resolving alone proves only the registration exists; the model is what the factory
        // reads out of the keyed config, so asserting it is what pins config to client
        // OPENAI001: Model is the SDK's only public window onto what the client was built with, and it is
        // marked evaluation-only. Reading it from a test is safe — if the SDK drops it, this test stops
        // compiling and nothing shipped is affected.
#pragma warning disable OPENAI001
        chat.Model.Is("gpt-5");
        audio.Model.Is("whisper-1");
#pragma warning restore OPENAI001
    }

    /// <summary>
    /// Client options carried by the configuration reach the built client: the factory falls back to the
    /// OpenAI defaults only when none were supplied.
    /// </summary>
    [Fact]
    public void RegisterConfiguration_Resolve_KeepsConfiguredClientOptions()
    {
        // arrange
        var endpoint = new Uri("https://openai.example.test/v1");
        var container = new ServiceContainer();
        container.AddOpenAI(
            "chat-client",
            _ => new OpenAIConfig("chatkey", "gpt-5", new OpenAIClientOptions { Endpoint = endpoint })
        );

        var provider = container.BuildServiceProvider();

        // act
        var client = provider.ResolveKeyed<OpenAIClient>("chat-client");

        // assert - the endpoint is the one part of the supplied options the SDK exposes, so it stands in
        // for the whole object: reaching it proves the options were not replaced by fresh defaults
#pragma warning disable OPENAI001
        client.Endpoint.Is(endpoint);
#pragma warning restore OPENAI001
    }

    /// <summary>
    /// One client per key, not one per resolution: the underlying <see cref="OpenAIClient"/> carries the
    /// HTTP pipeline, so rebuilding it on every resolution would churn connections.
    /// </summary>
    [Fact]
    public void AddOpenAI_ResolvedTwice_ReturnsTheSameClient()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddOpenAI("chat-client", _ => new OpenAIConfig("chatkey", "gpt-5", null));
        var provider = container.BuildServiceProvider();

        // act
        var first = provider.ResolveKeyed<OpenAIClient>("chat-client");
        var second = provider.ResolveKeyed<OpenAIClient>("chat-client");

        // assert
        ReferenceEquals(first, second).IsTrue("the client registration is a singleton per key");
    }

    /// <summary>
    /// The configuration delegate is stored, not invoked, at registration time: whatever it returns when
    /// the client is first resolved is what the client is built from. Once that has happened the client is
    /// a singleton, so later changes no longer reach it.
    /// </summary>
    [Fact]
    public void AddOpenAI_ConfigChangedAfterRegistration_IsReadAtFirstResolveOnly()
    {
        // arrange - the delegate reads a value the test moves underneath it
        var model = "gpt-5";
        var container = new ServiceContainer();
        container.AddOpenAI("chat-client", _ => new OpenAIConfig("chatkey", model, null));
        var provider = container.BuildServiceProvider();

        // act - changed after registration and after the provider was built, but before first resolution
        model = "gpt-5-mini";
        var client = provider.ResolveKeyed<ChatClient>("chat-client");

        // and changed again once the singleton exists
        model = "gpt-5-nano";
        var again = provider.ResolveKeyed<ChatClient>("chat-client");

        // assert - eager evaluation at registration would have frozen "gpt-5" here
#pragma warning disable OPENAI001
        client.Model.Is("gpt-5-mini");
        // the second half of the contract: deferral buys a late first read, not a live view
        again.Model.Is("gpt-5-mini");
#pragma warning restore OPENAI001
    }

    /// <summary>
    /// The container comes back from the call, so registrations chain off one expression and every one of
    /// them lands in the same container.
    /// </summary>
    [Fact]
    public void AddOpenAI_Chained_RegistersEveryClientInTheSameContainer()
    {
        // arrange & act - chaining is the whole point of the return value, so the test only ever holds the
        // result of the last call
        var provider = new ServiceContainer()
            .AddOpenAI("chat-client", _ => new OpenAIConfig("chatkey", "gpt-5", null))
            .AddOpenAI("audio-client", _ => new OpenAIConfig("audiokey", "whisper-1", null))
            .BuildServiceProvider();

        // assert - a call returning anything but its own container would drop the earlier registration
        provider.ResolveKeyed<ChatClient>("chat-client").IsNotDefault();
        provider.ResolveKeyed<AudioClient>("audio-client").IsNotDefault();
    }
}
