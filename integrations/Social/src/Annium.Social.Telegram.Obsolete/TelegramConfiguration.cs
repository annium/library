using System;

namespace Annium.Social.Telegram.Obsolete;

public record TelegramConfiguration
{
    public bool Debug { get; init; }
    public string ApiUrl { get; init; } = string.Empty;
    public string SkipMessage { get; init; } = string.Empty;
    public IFormatProvider DateTimeFormat { get; init; } = default!;
    public string MonthFormat { get; init; } = string.Empty;
    public string DateFormat { get; init; } = string.Empty;
    public string TimeFormat { get; init; } = string.Empty;
}
