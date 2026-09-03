using System.Threading.Tasks;
using Annium.Social.Telegram.Integration;
using Annium.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Social.Telegram.Handlers;

/// <summary>
/// Handles a single Telegram update (e.g. an incoming message), with access to the bot's API client for replying.
/// </summary>
public interface ITelegramMessageHandler
{
    /// <summary>
    /// Processes the given update, optionally using <paramref name="api"/> to send a response back to Telegram.
    /// </summary>
    /// <param name="update">The update to process.</param>
    /// <param name="api">The API client for sending messages back to Telegram.</param>
    /// <returns>A task that completes once the update has been processed.</returns>
    Task ProcessAsync(Update update, ITelegramApi api);
}
