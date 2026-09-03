using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramMenuFactory
{
    Task<ITelegramMenu> CreateAsync(int userId);

    bool IsCancel(TelegramMessage message);
}
