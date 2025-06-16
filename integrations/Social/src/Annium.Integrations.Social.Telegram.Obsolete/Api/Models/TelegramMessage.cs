namespace Annium.Integrations.Social.Telegram.Obsolete.Api.Models;

public class TelegramMessage
{
    public int MessageId { get; set; }

    public TelegramUser From { get; set; } = new();

    public int Date { get; set; }

    public int EditDate { get; set; }

    public TelegramChat Chat { get; set; } = new();

    public string Text { get; set; } = string.Empty;

    public TelegramMessageEntity[] Entities { get; set; } = [];
}
