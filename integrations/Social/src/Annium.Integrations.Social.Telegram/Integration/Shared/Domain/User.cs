using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

/// <summary>
/// A Telegram user or bot, as reported by the Bot API (e.g. as a message's sender).
/// </summary>
public sealed record User
{
    /// <summary>
    /// The user's unique Telegram identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    /// <summary>
    /// Whether this user is a bot rather than a human account.
    /// </summary>
    [JsonPropertyName("is_bot")]
    public required bool IsBot { get; init; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    [JsonPropertyName("first_name")]
    public required string FirstName { get; init; }

    /// <summary>
    /// The user's last name, if set.
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    /// <summary>
    /// The user's public @username, if they have one.
    /// </summary>
    /// <remarks>
    /// Omitted entirely by Telegram when the user has no public username, so it cannot be required.
    /// </remarks>
    [JsonPropertyName("username")]
    public string? Username { get; init; }
}
