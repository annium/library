using Annium.Integrations.Social.Telegram.Integration.Messages;

namespace Annium.Integrations.Social.Telegram.Integration;

public interface ITelegramApi
{
    IMessageApi Messages { get; }
}
