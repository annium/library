using Annium.Core.DependencyInjection;
using Annium.Integrations.AI.OpenAI;
using Annium.Logging.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace Annium.Integrations.AI.SemanticKernel.Tests;

public class ServiceContainerExtensionsTests
{
    [Fact]
    public void RegisterConfiguration_Resolve_Works()
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

        // assert
        provider.ResolveKeyed<IChatCompletionService>("chat-client");
#pragma warning disable SKEXP0001
        provider.ResolveKeyed<IAudioToTextService>("audio-client");
#pragma warning restore SKEXP0001
    }
}
