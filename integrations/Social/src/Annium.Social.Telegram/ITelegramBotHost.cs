using System.Threading;
using System.Threading.Tasks;

namespace Annium.Social.Telegram;

/// <summary>
/// Runs a configured Telegram bot instance: starts its receiver and dispatches incoming updates to the registered
/// handler.
/// </summary>
public interface ITelegramBotHost
{
    /// <summary>
    /// Starts processing updates from the bot's receiver until <paramref name="ct"/> is canceled.
    /// </summary>
    /// <param name="ct">The token used to stop processing.</param>
    /// <returns>A task that completes once processing has stopped.</returns>
    Task RunAsync(CancellationToken ct);
}
