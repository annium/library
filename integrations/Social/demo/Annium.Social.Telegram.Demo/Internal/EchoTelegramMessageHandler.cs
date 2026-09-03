using System.Threading.Tasks;
using Annium.Logging;
using Annium.Social.Telegram.Handlers;
using Annium.Social.Telegram.Integration;
using Annium.Social.Telegram.Integration.Messages.Requests;
using Annium.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Social.Telegram.Demo.Internal;

/// <summary>
/// Demo handler replying to every text message with the same text, and ignoring updates that carry none.
/// </summary>
internal class EchoTelegramMessageHandler : ITelegramMessageHandler, ILogSubject
{
    /// <summary>
    /// The logger used to trace handling of each update.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Creates the handler.
    /// </summary>
    /// <param name="logger">The logger used to trace handling of each update.</param>
    public EchoTelegramMessageHandler(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Echoes the text of the update's message back to its chat, skipping updates without text
    /// (photos, stickers, service messages).
    /// </summary>
    /// <param name="update">The update to handle.</param>
    /// <param name="api">The API used to reply.</param>
    /// <returns>A task that completes once the reply has been sent or the update skipped.</returns>
    public async Task ProcessAsync(Update update, ITelegramApi api)
    {
        this.Trace("start");

        var message = update.Message ?? update.EditedMessage;
        if (message?.Text is null)
        {
            this.Trace("noop - no text message to echo");
            return;
        }

        await api.Messages.SendMessageAsync(new SendMessageRequest { ChatId = message.Chat.Id, Text = message.Text });

        this.Trace("done");
    }
}
