using OpenAI;

namespace Annium.Integrations.AI.OpenAI;

public sealed record OpenAiConfig(string Key, string Model, OpenAIClientOptions? Options);
