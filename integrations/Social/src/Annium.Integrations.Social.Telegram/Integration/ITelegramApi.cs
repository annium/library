using Annium.Integrations.Social.Telegram.Integration.Messages;

namespace Annium.Integrations.Social.Telegram.Integration;

/// <summary>
/// Client for calling the Telegram Bot API for a single configured bot.
/// </summary>
public interface ITelegramApi
{
    /// <summary>
    /// Gets the message-related operations (e.g. sending messages) exposed by the Telegram Bot API.
    /// </summary>
    IMessageApi Messages { get; }
}
