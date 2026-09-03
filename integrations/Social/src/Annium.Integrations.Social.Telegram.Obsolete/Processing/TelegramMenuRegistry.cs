using System;
using System.Collections.Generic;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public class TelegramMenuRegistry : ITelegramMenuRegistry
{
    private readonly IDictionary<int, ValueTuple<ITelegramMenu, ITelegramUserCommandProcessor>> _data =
        new Dictionary<int, ValueTuple<ITelegramMenu, ITelegramUserCommandProcessor>>();

    public bool HasData(int userId)
    {
        lock (_data)
            return _data.ContainsKey(userId);
    }

    public ValueTuple<ITelegramMenu, ITelegramUserCommandProcessor> GetData(int userId)
    {
        lock (_data)
            return _data.ContainsKey(userId) ? _data[userId] : default;
    }

    public void SetData(int userId, ITelegramMenu operation, ITelegramUserCommandProcessor processor)
    {
        lock (_data)
            _data[userId] = (operation, processor);
    }

    public void ClearData(int userId)
    {
        lock (_data)
            _data.Remove(userId);
    }
}
