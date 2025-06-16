using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Messages.Requests;

public sealed record SendMessageRequest
{
    [JsonPropertyName("chat_id")]
    public required long ChatId { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}
