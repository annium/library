using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration;
using Annium.Integrations.Social.Telegram.Integration.Shared.Domain;

namespace Annium.Integrations.Social.Telegram.Handlers;

public interface ITelegramMessageHandler
{
    Task ProcessAsync(Update update, ITelegramApi api);
}
