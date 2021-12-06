using System;
using Annium.Core.DependencyInjection;

namespace Backuper.Storage;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<StorageFactory>().Singleton();

        container.AddLogging(route => route.UseConsole());
    }
}