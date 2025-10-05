using OpenAI;

namespace Annium.Integrations.AI.OpenAI;

// ReSharper disable once InconsistentNaming
public sealed record OpenAIConfig(string Key, string Model, OpenAIClientOptions? Options);
