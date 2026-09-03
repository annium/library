using System;

namespace Annium.Integrations.Social.Telegram;

/// <summary>
/// Configuration for a single Telegram bot instance: its API token and, optionally, webhook settings.
/// </summary>
public sealed record TelegramBotConfiguration
{
    /// <summary>
    /// The bot's Telegram API token, used to build its API base URL. Treat as a secret; never log it.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// The webhook configuration to use instead of long-polling, or <see langword="null"/> to receive updates via
    /// polling.
    /// </summary>
    public TelegramBotWebhookConfiguration? Webhook { get; init; }

    /// <summary>
    /// Renders the configuration with the token redacted: a record's generated ToString prints every property,
    /// so any log line or exception message carrying this object would otherwise expose the bot's credential.
    /// </summary>
    /// <returns>The configuration with the token replaced by a placeholder.</returns>
    public override string ToString() =>
        $"{nameof(TelegramBotConfiguration)} {{ {nameof(Token)} = ***, {nameof(Webhook)} = {Webhook} }}";
}

/// <summary>
/// Configuration for receiving Telegram updates via a webhook instead of polling.
/// </summary>
public sealed record TelegramBotWebhookConfiguration
{
    /// <summary>
    /// The local port the webhook HTTP server listens on.
    /// </summary>
    public ushort InternalPort { get; init; }

    /// <summary>
    /// The externally reachable URL registered with Telegram as the webhook target.
    /// </summary>
    public Uri? ExternalAddress { get; init; }

    /// <summary>
    /// The secret token Telegram must echo back in each webhook request, used to verify that pushes originate from
    /// Telegram. Treat as a secret; never log it.
    /// </summary>
    public string SecretToken { get; init; } = string.Empty;

    /// <summary>
    /// Renders the configuration with the secret token redacted, for the same reason as the bot token above.
    /// </summary>
    /// <returns>The configuration with the secret token replaced by a placeholder.</returns>
    public override string ToString() =>
        $"{nameof(TelegramBotWebhookConfiguration)} {{ {nameof(InternalPort)} = {InternalPort}, "
        + $"{nameof(ExternalAddress)} = {ExternalAddress}, {nameof(SecretToken)} = *** }}";
}
