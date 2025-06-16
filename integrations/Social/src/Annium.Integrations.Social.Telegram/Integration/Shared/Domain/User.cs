using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

public sealed record User
{
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    [JsonPropertyName("is_bot")]
    public required bool IsBot { get; init; }

    [JsonPropertyName("first_name")]
    public required string FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("username")]
    public required string? Username { get; init; }
}
