using System.Text.Json.Serialization;

namespace Annium.Social.Telegram.Integration.Messages.Requests;

/// <summary>
/// Request body for the Telegram <c>sendMessage</c> API call.
/// </summary>
public sealed record SendMessageRequest
{
    /// <summary>
    /// The unique identifier of the target chat to send the message to.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public required long ChatId { get; init; }

    /// <summary>
    /// The text content of the message to send.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}
