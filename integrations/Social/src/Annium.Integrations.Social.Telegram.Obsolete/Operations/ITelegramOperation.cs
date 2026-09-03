using System.Threading;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Obsolete.Processing;

namespace Annium.Integrations.Social.Telegram.Obsolete.Operations;

public interface ITelegramOperation
{
    string Description { get; }

    Task RunAsync(int userId, ITelegramUserProcessor processor, CancellationToken token);
}
