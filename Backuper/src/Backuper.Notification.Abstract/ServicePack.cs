using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace Backuper.Notification.Abstract;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ChannelFactory>().Singleton();

        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
