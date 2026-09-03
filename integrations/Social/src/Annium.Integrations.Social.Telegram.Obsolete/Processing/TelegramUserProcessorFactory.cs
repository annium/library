using Annium.Integrations.Social.Telegram.Obsolete.Api;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public class TelegramUserProcessorFactory : ITelegramUserProcessorFactory
{
    private readonly ITelegramApi _api;
    private readonly TelegramConfiguration _configuration;

    public TelegramUserProcessorFactory(ITelegramApi api, TelegramConfiguration configuration)
    {
        _api = api;
        _configuration = configuration;
    }

    public ITelegramUserCommandProcessor Create(int userId) => new TelegramUserProcessor(_api, userId, _configuration);
}
