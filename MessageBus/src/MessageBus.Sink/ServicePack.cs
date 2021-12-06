using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Serialization.Abstractions;

namespace MessageBus.Sink;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddRuntimeTools(GetType().Assembly, true);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging(route => route.UseConsole());
        container.AddJsonSerializers().SetDefault();
        container.AddMapper();
        container.AddConfiguration<EndpointsConfiguration>(x => x
            .AddYamlFile("configuration.yml")
            .AddCommandLineArgs()
        );
        container.AddNetMQMessageBus((sp, opts) => opts
            .WithSerializer(sp.Resolve<ISerializer<string>>())
            .WithEndpoints(sp.Resolve<EndpointsConfiguration>())
        );
    }
}