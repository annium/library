using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

public sealed record Chat
{
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    [JsonPropertyName("type")]
    public required ChatType Type { get; init; }
}
