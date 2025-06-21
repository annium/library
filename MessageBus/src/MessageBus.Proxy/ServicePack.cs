using System;
using Annium.Configuration.Abstractions;
using Annium.Configuration.CommandLine;
using Annium.Configuration.Yaml;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;

namespace MessageBus.Proxy;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddMapper();
        container.AddConfiguration<Configuration>(x => x.AddYamlFile("configuration.yml").AddCommandLineArgs());
    }
}
