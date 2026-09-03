namespace Annium.Social.Telegram.Obsolete.Api.Models;

public class TelegramUpdate
{
    public int UpdateId { get; set; }

    public TelegramMessage Message { get; set; } = new();
}
