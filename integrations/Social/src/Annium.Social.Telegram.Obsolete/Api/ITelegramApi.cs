using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Social.Telegram.Obsolete.Api;

public interface ITelegramApi
{
    Task<TelegramApiResult<TelegramUser>> GetMeAsync();

    Task<TelegramApiResult<TelegramUpdate[]>> GetUpdatesAsync(int offset = 0);

    Task<TelegramApiResult<TelegramMessage>> SendMessageAsync(
        int chatId,
        string text,
        IReadOnlyList<string>? buttons = null
    );

    Task<TelegramApiResult<TelegramMessage>> SendMessageAsync(
        int chatId,
        IReadOnlyList<string> text,
        IReadOnlyList<string>? buttons = null
    );
}
