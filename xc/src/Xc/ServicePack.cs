using System;
using Annium.Core.DependencyInjection;
using Xc.Tasks;

namespace Xc;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();

        container.AddAll(GetType().Assembly)
            .AssignableTo<ITask>()
            .Where(x => x.IsClass)
            .AsSelf()
            .AsSelfFactory()
            .Transient();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}