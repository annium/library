using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace Backuper.Storage;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<StorageFactory>().Singleton();

        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
