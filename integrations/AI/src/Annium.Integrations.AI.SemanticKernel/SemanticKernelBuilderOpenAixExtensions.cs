using System.Diagnostics.CodeAnalysis;
using Annium.Core.DependencyInjection;
using Annium.Integrations.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using OpenAI;

namespace Annium.Integrations.AI.SemanticKernel;

// ReSharper disable InconsistentNaming
public static class SemanticKernelBuilderOpenAixExtensions
{
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
