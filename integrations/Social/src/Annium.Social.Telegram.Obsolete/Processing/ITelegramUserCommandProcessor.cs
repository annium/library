using Annium.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Social.Telegram.Obsolete.Processing;

public interface ITelegramUserCommandProcessor : ITelegramUserProcessor
{
    void HandleMessage(TelegramMessage message);
}
