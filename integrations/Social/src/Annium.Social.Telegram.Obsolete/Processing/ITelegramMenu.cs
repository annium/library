using System.Threading;
using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Operations;

namespace Annium.Social.Telegram.Obsolete.Processing;

public interface ITelegramMenu
{
    ITelegramMenu BeginCategory(string name);

    ITelegramMenu AddOperation<TOperation>()
        where TOperation : ITelegramOperation;

    ITelegramMenu EndCategory();

    Task<ITelegramOperation> GetOperationAsync(int userId, ITelegramUserProcessor processor, CancellationToken token);
}
