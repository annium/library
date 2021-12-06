using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;

namespace MessageBus.Proxy;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddRuntimeTools(GetType().Assembly, true);
        container.AddMapper();
        container.AddConfiguration<Configuration>(x => x
            .AddYamlFile("configuration.yml")
            .AddCommandLineArgs()
        );
    }
}