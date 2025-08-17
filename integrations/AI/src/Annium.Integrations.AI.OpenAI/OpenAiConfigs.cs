using System.Collections.Generic;
using OpenAI;

namespace Annium.Integrations.AI.OpenAI;

public sealed class OpenAiConfigs : Dictionary<string, OpenAiConfig>;

public sealed record OpenAiConfig(string Key, string Model, OpenAIClientOptions? Options);
