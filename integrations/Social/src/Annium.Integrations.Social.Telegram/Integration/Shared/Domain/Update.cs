using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// A single Telegram update returned by <c>getUpdates</c> or pushed to a webhook, carrying at most one new or
/// edited message.
/// </summary>
public sealed record Update
{
    /// <summary>
    /// The update's sequence number. The next <c>getUpdates</c> poll passes <c>offset</c> as this value plus one,
    /// confirming receipt of this and all earlier updates.
    /// </summary>
    [JsonPropertyName("update_id")]
    public required long Id { get; init; }

    /// <summary>
    /// The new incoming message carried by this update, if any.
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; init; }

    /// <summary>
    /// The edited message carried by this update, if any.
    /// </summary>
    [JsonPropertyName("edited_message")]
    public Message? EditedMessage { get; init; }
}
