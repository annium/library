using System.Threading.Tasks;

namespace Annium.Social.Telegram.Obsolete.Processing;

public interface ITelegramProcessor
{
    Task PollUpdatesAsync();
}
