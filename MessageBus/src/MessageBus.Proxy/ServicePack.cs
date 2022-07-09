using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;

namespace MessageBus.Proxy;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddMapper();
        container.AddConfiguration<Configuration>(x => x
            .AddYamlFile("configuration.yml")
            .AddCommandLineArgs()
        );
    }
}