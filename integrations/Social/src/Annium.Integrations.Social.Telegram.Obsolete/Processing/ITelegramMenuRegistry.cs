using System;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramMenuRegistry
{
    bool HasData(int userId);

    ValueTuple<ITelegramMenu, ITelegramUserCommandProcessor> GetData(int userId);

    void SetData(int userId, ITelegramMenu menu, ITelegramUserCommandProcessor processor);

    void ClearData(int userId);
}
