using Annium.AI.OpenAI;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;

namespace Annium.AI.SemanticKernel.Tests;

/// <summary>
/// Tests that OpenAI-backed kernel services resolve under the client key they were registered with.
/// </summary>
public class ServiceContainerExtensionsTests
{
    /// <summary>
    /// Chat completion and audio transcription resolve for their respective OpenAI clients, each bound to
    /// the model named by the configuration registered under its key.
    /// </summary>
    [Fact]
    public void RegisterConfiguration_Resolve_BindsEachServiceToItsConfiguredModel()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddLogging();
        container.Collection.AddLogging();
        container.AddOpenAI("chat-client", _ => new OpenAIConfig("chatkey", "gpt-5", null));
        container.AddOpenAI("audio-client", _ => new OpenAIConfig("audiokey", "whisper-1", null));
#pragma warning disable SKEXP0001
        container.AddSemanticKernel().WithOpenAIChatCompletion("chat-client").WithOpenAIAudioToText("audio-client");
#pragma warning restore SKEXP0001

        var provider = container.BuildServiceProvider();

        // act
        var chat = provider.ResolveKeyed<IChatCompletionService>("chat-client");
#pragma warning disable SKEXP0001
        var audio = provider.ResolveKeyed<IAudioToTextService>("audio-client");
#pragma warning restore SKEXP0001

        // assert - resolving alone proves only that the registration exists; the model is what the factory
        // reads out of the keyed OpenAI config, so asserting it is what ties config to service
        chat.GetModelId().Is("gpt-5");
        audio.GetModelId().Is("whisper-1");
        // the chat registration is published under two interfaces; text generation is the quieter one and
        // would go unnoticed if the second AsKeyed call were dropped
        provider.ResolveKeyed<ITextGenerationService>("chat-client").GetModelId().Is("gpt-5");
    }
}
