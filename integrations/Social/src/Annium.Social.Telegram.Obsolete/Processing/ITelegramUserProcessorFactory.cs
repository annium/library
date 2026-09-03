namespace Annium.Social.Telegram.Obsolete.Processing;

public interface ITelegramUserProcessorFactory
{
    ITelegramUserCommandProcessor Create(int userId);
}
