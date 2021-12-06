using System;
using Annium.Core.DependencyInjection;

namespace Backuper.Connection.Abstract;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<ConnectionFactory>().Singleton();

        container.AddLogging(route => route.UseConsole());
        container.AddShell();
    }
}