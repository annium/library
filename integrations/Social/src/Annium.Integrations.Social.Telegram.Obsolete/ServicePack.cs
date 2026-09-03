using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Social.Telegram.Obsolete.Api;
using Annium.Integrations.Social.Telegram.Obsolete.Processing;

namespace Annium.Integrations.Social.Telegram.Obsolete;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.Add<ITelegramApi, TelegramApi>().Singleton();
        container.Add<ITelegramProcessor, TelegramProcessor>().Singleton();
        container.Add<ITelegramMenu, TelegramMenu>().Transient();
        container.Add<ITelegramMenuRegistry, TelegramMenuRegistry>().Singleton();
        container.Add<ITelegramProcessorRegistry, TelegramProcessorRegistry>().Singleton();
        container.Add<ITelegramUserProcessorFactory, TelegramUserProcessorFactory>().Singleton();

        return Task.CompletedTask;
    }
}
