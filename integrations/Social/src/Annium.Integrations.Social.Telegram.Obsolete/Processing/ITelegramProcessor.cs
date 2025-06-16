using System.Threading.Tasks;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramProcessor
{
    Task PollUpdatesAsync();
}
