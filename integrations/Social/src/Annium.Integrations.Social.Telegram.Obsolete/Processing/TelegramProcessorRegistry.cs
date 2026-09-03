using System;
using System.Collections.Generic;
using System.Threading;
using Annium.Integrations.Social.Telegram.Obsolete.Operations;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public class TelegramProcessorRegistry : ITelegramProcessorRegistry
{
    private readonly IDictionary<
        int,
        ValueTuple<ITelegramOperation, ITelegramUserCommandProcessor, CancellationTokenSource>
    > _data =
        new Dictionary<int, ValueTuple<ITelegramOperation, ITelegramUserCommandProcessor, CancellationTokenSource>>();

    public bool HasData(int userId)
    {
        lock (_data)
            return _data.ContainsKey(userId);
    }

    public ValueTuple<ITelegramOperation, ITelegramUserCommandProcessor, CancellationTokenSource> GetData(int userId)
    {
        lock (_data)
            return _data.ContainsKey(userId) ? _data[userId] : default;
    }

    public void SetData(
        int userId,
        ITelegramOperation operation,
        ITelegramUserCommandProcessor processor,
        CancellationTokenSource cts
    )
    {
        lock (_data)
            _data[userId] = (operation, processor, cts);
    }

    public void ClearData(int userId)
    {
        lock (_data)
            _data.Remove(userId);
    }
}
