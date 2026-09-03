using OpenAI;

namespace Annium.Integrations.AI.OpenAI;

/// <summary>
/// Connection settings for a single OpenAI client: the API key, the model it defaults to, and optional
/// client options.
/// </summary>
/// <param name="Key">The OpenAI API key. Never rendered by <see cref="ToString"/>.</param>
/// <param name="Model">The model id requests default to (e.g. gpt-5, whisper-1).</param>
/// <param name="Options">Client options (endpoint, retries, …), or null for the OpenAI defaults.</param>
// ReSharper disable once InconsistentNaming
public sealed record OpenAIConfig(string Key, string Model, OpenAIClientOptions? Options)
{
    /// <summary>
    /// Renders the config with the API key redacted: a record's generated ToString prints every property,
    /// so any log line or exception message carrying this object would otherwise expose the key.
    /// </summary>
    /// <returns>The config with the key replaced by a placeholder.</returns>
    public override string ToString() =>
        $"{nameof(OpenAIConfig)} {{ {nameof(Key)} = ***, {nameof(Model)} = {Model}, {nameof(Options)} = {Options} }}";
}
