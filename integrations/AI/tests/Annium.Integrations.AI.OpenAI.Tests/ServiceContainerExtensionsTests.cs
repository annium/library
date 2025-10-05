using Annium.Core.DependencyInjection;
using OpenAI.Audio;
using OpenAI.Chat;
using Xunit;

namespace Annium.Integrations.AI.OpenAI.Tests;

public class ServiceContainerExtensionsTests
{
    [Fact]
    public void RegisterConfiguration_Resolve_Works()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddOpenAi("chat-client", _ => new OpenAiConfig("chatkey", "gpt-5", null));
        container.AddOpenAi("audio-client", _ => new OpenAiConfig("audiokey", "whisper-1", null));

        var provider = container.BuildServiceProvider();

        // assert
        provider.ResolveKeyed<ChatClient>("chat-client");
        provider.ResolveKeyed<AudioClient>("audio-client");
    }
}
