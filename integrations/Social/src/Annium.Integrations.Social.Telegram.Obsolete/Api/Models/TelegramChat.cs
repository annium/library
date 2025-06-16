namespace Annium.Integrations.Social.Telegram.Obsolete.Api.Models;

public class TelegramChat
{
    public int Id { get; set; }

    public TelegramChatType Type { get; set; }

    public string Username { get; set; } = string.Empty;
}

public enum TelegramChatType
{
    Private,

    Group,

    Supergroup,

    Channel,
}
