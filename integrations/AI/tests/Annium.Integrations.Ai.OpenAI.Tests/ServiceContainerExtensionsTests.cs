using Annium.Core.DependencyInjection;
using Annium.Integrations.AI.OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using Xunit;

namespace Annium.Integrations.Ai.OpenAI.Tests;

public class ServiceContainerExtensionsTests
{
    [Fact]
    public void RegisterConfiguration_Resolve_Works()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddOpenAi(
            new()
            {
                { "chat-client", new OpenAiConfig("chatkey", "gpt-4o", null) },
                { "audio-client", new OpenAiConfig("audiokey", "whisper-1", null) },
            }
        );

        var provider = container.BuildServiceProvider();

        // assert
        provider.ResolveKeyed<ChatClient>("chat-client");
        provider.ResolveKeyed<AudioClient>("audio-client");
    }
}
