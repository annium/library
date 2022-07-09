using System;
using Annium.Core.DependencyInjection;

namespace Xmg;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Configuration.ServicePack>();
        Add<Migration.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddMapper();
        container.AddArguments();
        container.AddJsonSerializers().SetDefault();
        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}