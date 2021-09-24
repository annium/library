using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;
using XSass.Internal;
using XSass.Internal.Components;

namespace XSass
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddMapper();
            container.AddConfiguration<Configuration>(x => x.AddYamlFile("xsass.yml", true));
            container.AddTimeProvider();
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            Action<LogRoute<DefaultLogContext>> logRoute = provider.Resolve<Configuration>().Debug ? route => route.UseConsole() : _ => { };
            container.AddLogging(logRoute);
            container.Add<Compiler>().AsSelf().Singleton();
            container.Add<Crawler>().AsSelf().Singleton();
        }
    }
}