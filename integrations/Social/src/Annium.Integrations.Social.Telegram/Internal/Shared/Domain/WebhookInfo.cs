using System;
using System.Text.Json.Serialization;

namespace Annium.Integrations.Social.Telegram.Internal.Shared.Domain;

/// <summary>
/// Response payload for Telegram's <c>getWebhookInfo</c> call, describing the currently registered webhook.
/// </summary>
internal sealed record WebhookInfo
{
    /// <summary>
    /// The webhook URL currently registered with Telegram, or <see langword="null"/> if none is set.
    /// </summary>
    [JsonPropertyName("url")]
    public required Uri? Url { get; init; }
}
