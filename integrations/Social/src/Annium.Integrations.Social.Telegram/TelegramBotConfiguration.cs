using System;

namespace Annium.Integrations.Social.Telegram;

public sealed record TelegramBotConfiguration
{
    public string Token { get; init; } = string.Empty;
    public TelegramBotWebhookConfiguration? Webhook { get; init; }
}

public sealed record TelegramBotWebhookConfiguration
{
    public int InternalPort { get; init; }
    public Uri? ExternalAddress { get; init; }
    public string SecretToken { get; init; } = string.Empty;
}
