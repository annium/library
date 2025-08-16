using System.Collections.Generic;

namespace Annium.Integrations.AI.OpenAI;

public sealed class OpenAiConfigs : Dictionary<string, OpenAiConfig>;

public sealed record OpenAiConfig(string Key, string Model);
