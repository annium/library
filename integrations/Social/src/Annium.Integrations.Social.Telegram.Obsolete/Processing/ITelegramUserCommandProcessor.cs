using Annium.Integrations.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramUserCommandProcessor : ITelegramUserProcessor
{
    void HandleMessage(TelegramMessage message);
}
