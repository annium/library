using System;
using Annium.Core.DependencyInjection;
using xdomains.Tools;

namespace xdomains;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddMapper();

        container.Add<Cache>().AsSelf().Singleton();
        container.Add<Parser>().AsSelf().Singleton();
        container.Add<Resolver>().AsSelf().Singleton();
        container.Add<Worker>().AsSelf().Singleton();

        container.Add<Settings>().AsSelf().Singleton();

        container.AddArguments();
        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
