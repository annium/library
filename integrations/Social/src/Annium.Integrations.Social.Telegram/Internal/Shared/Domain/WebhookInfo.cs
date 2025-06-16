using System;
using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Internal.Shared.Domain;

internal sealed record WebhookInfo
{
    [JsonPropertyName("url")]
    public required Uri? Url { get; init; }
}
