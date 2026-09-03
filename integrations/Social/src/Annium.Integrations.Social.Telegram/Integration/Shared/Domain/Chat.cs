using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// A Telegram chat (private conversation, group, supergroup, or channel) a message belongs to.
/// </summary>
public sealed record Chat
{
    /// <summary>
    /// The unique identifier of the chat.
    /// </summary>
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    /// <summary>
    /// The kind of chat (private, group, supergroup, or channel).
    /// </summary>
    [JsonPropertyName("type")]
    public required ChatType Type { get; init; }
}
