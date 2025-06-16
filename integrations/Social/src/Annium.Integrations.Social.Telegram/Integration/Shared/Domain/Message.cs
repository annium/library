using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

public sealed record Message
{
    [JsonPropertyName("message_id")]
    public required int Id { get; init; }

    [JsonPropertyName("from")]
    public required User From { get; init; }

    [JsonPropertyName("chat")]
    public required Chat Chat { get; init; }

    [JsonPropertyName("date")]
    public long? Date { get; set; }

    [JsonPropertyName("edit_date")]
    public long? EditDate { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}
