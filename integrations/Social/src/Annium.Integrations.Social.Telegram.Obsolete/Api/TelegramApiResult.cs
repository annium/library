namespace Annium.Integrations.Social.Telegram.Obsolete.Api;

public class TelegramApiResult<TResult>
{
    public required bool Ok { get; init; }

    public required TResult Result { get; init; }

    public required string Description { get; init; }
}
