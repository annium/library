using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// A Telegram message, as delivered inside an <see cref="Update"/> (new or edited).
/// </summary>
public sealed record Message
{
    /// <summary>
    /// The Telegram-assigned identifier of the message within its chat.
    /// </summary>
    [JsonPropertyName("message_id")]
    public required int Id { get; init; }

    /// <summary>
    /// The user who sent the message.
    /// </summary>
    /// <remarks>
    /// Absent for messages sent to a channel — Telegram only fills it for messages from a user.
    /// </remarks>
    [JsonPropertyName("from")]
    public User? From { get; init; }

    /// <summary>
    /// The chat the message was sent to.
    /// </summary>
    [JsonPropertyName("chat")]
    public required Chat Chat { get; init; }

    /// <summary>
    /// Unix timestamp of when the message was sent.
    /// </summary>
    [JsonPropertyName("date")]
    public long? Date { get; init; }

    /// <summary>
    /// Unix timestamp of the message's most recent edit, if it has been edited.
    /// </summary>
    [JsonPropertyName("edit_date")]
    public long? EditDate { get; init; }

    /// <summary>
    /// The message's text content.
    /// </summary>
    /// <remarks>
    /// Absent for every non-text message (photo, sticker, document, …). Declaring it required made
    /// deserialization of the whole update batch fail on the first such message.
    /// </remarks>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
