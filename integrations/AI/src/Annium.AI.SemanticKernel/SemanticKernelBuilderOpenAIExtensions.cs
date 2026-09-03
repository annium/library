using System.Diagnostics.CodeAnalysis;
using Annium.Core.DependencyInjection;
using Annium.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using OpenAI;

namespace Annium.AI.SemanticKernel;

// ReSharper disable InconsistentNaming
/// <summary>
/// Builder extensions wiring OpenAI-backed Semantic Kernel services to a client registered via <c>AddOpenAI</c>.
/// </summary>
public static class SemanticKernelBuilderOpenAIExtensions
{
    /// <summary>
    /// Registers chat completion and text generation backed by the OpenAI client and model configured under
    /// <paramref name="clientId"/>.
    /// </summary>
    /// <param name="builder">The kernel builder to register into.</param>
    /// <param name="clientId">The key of the OpenAI client registration to use.</param>
    /// <returns>The builder, for chaining.</returns>
    public static ISemanticKernelBuilder WithOpenAIChatCompletion(this ISemanticKernelBuilder builder, string clientId)
    {
        builder
            .Container.Add<OpenAIChatCompletionService>(
                (sp, key) =>
                {
                    var config = sp.ResolveKeyed<GetOpenAIConfig>(key)(sp);
                    var modelId = config.Model;
                    var client = sp.ResolveKeyed<OpenAIClient>(clientId);
                    var loggerFactory = sp.Resolve<ILoggerFactory>();

                    return new(modelId, client, loggerFactory);
                }
            )
            .AsKeyed<IChatCompletionService>(clientId)
            .AsKeyed<ITextGenerationService>(clientId)
            .Singleton();

        return builder;
    }

    /// <summary>
    /// Registers audio transcription backed by the OpenAI client and model configured under
    /// <paramref name="clientId"/>.
    /// </summary>
    /// <param name="builder">The kernel builder to register into.</param>
    /// <param name="clientId">The key of the OpenAI client registration to use.</param>
    /// <returns>The builder, for chaining.</returns>
    [Experimental("SKEXP0001")]
    public static ISemanticKernelBuilder WithOpenAIAudioToText(this ISemanticKernelBuilder builder, string clientId)
    {
        builder
            .Container.Add<OpenAIAudioToTextService>(
                (sp, key) =>
                {
                    var config = sp.ResolveKeyed<GetOpenAIConfig>(key)(sp);
                    var modelId = config.Model;
                    var client = sp.ResolveKeyed<OpenAIClient>(clientId);
                    var loggerFactory = sp.Resolve<ILoggerFactory>();

                    return new(modelId, client, loggerFactory);
                }
            )
            .AsKeyed<IAudioToTextService>(clientId)
            .Singleton();

        return builder;
    }
}
