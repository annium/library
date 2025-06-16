using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Handlers;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Messages.Requests;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;
using Annium.Logging;

namespace Annium.Integrations.Social.Telegram.Demo.Internal;

internal class EchoTelegramMessageHandler : ITelegramMessageHandler, ILogSubject
{
    public ILogger Logger { get; }

    public EchoTelegramMessageHandler(ILogger logger)
    {
        Logger = logger;
    }

    public async Task ProcessAsync(Update update, ITelegramApi api)
    {
        this.Trace("start");

        var message = update.Message ?? update.EditedMessage;
        if (message is null)
        {
            this.Trace("noop - no message to echo");
            return;
        }

        await api.Messages.SendMessageAsync(new SendMessageRequest { ChatId = message.Chat.Id, Text = message.Text });

        this.Trace("done");
    }
}
