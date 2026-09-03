using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Social.Telegram.Obsolete.Processing;

public interface ITelegramMenuFactory
{
    Task<ITelegramMenu> CreateAsync(int userId);

    bool IsCancel(TelegramMessage message);
}
