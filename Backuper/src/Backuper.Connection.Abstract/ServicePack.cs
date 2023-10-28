using System;
using Annium.Core.DependencyInjection;

namespace Backuper.Connection.Abstract;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ConnectionFactory>().Singleton();

        container.AddLogging();
        container.AddShell();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
