using System.Threading;
using System.Threading.Tasks;

namespace Annium.Integrations.Social.Telegram;

public interface ITelegramBotHost
{
    Task RunAsync(CancellationToken ct);
}
