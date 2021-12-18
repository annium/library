using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;
using XSass.Internal;
using XSass.Internal.Components;

namespace XSass;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntimeTools(GetType().Assembly, false);
        container.AddMapper();
        container.AddConfiguration<Configuration>(x => x.AddYamlFile("xsass.yml", true));
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.Add<Compiler>().AsSelf().Singleton();
        container.Add<Crawler>().AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        Action<LogRoute<DefaultLogContext>> logRoute = provider.Resolve<Configuration>().Debug ? route => route.UseConsole() : _ => { };
        provider.UseLogging(logRoute);
    }
}