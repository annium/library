using System;
using System.Threading;
using Annium.Integrations.Social.Telegram.Obsolete.Operations;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramProcessorRegistry
{
    bool HasData(int userId);

    ValueTuple<ITelegramOperation, ITelegramUserCommandProcessor, CancellationTokenSource> GetData(int userId);

    void SetData(
        int userId,
        ITelegramOperation operation,
        ITelegramUserCommandProcessor processor,
        CancellationTokenSource cts
    );

    void ClearData(int userId);
}
