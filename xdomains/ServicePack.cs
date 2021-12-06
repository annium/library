using System;
using Annium.Core.DependencyInjection;
using xdomains.Tools;

namespace xdomains;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntimeTools(GetType().Assembly, false);
        container.AddTime().WithRealTime().SetDefault();
        container.AddMapper();

        container.Add<Cache>().AsSelf().Singleton();
        container.Add<Parser>().AsSelf().Singleton();
        container.Add<Resolver>().AsSelf().Singleton();
        container.Add<Worker>().AsSelf().Singleton();

        container.Add<Settings>().AsSelf().Singleton();

        container.AddArguments();
        container.AddLogging(route => route.UseConsole());
    }
}