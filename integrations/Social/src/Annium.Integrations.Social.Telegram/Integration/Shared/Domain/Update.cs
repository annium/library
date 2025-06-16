using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

public sealed record Update
{
    [JsonPropertyName("update_id")]
    public required long Id { get; init; }

    [JsonPropertyName("message")]
    public Message? Message { get; init; }

    [JsonPropertyName("edited_message")]
    public Message? EditedMessage { get; init; }
}
