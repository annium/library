using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Serialization.Abstractions;

namespace MessageBus.Sink;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddSerializers().WithJson(isDefault: true);

        container.AddMapper();
        container.AddConfiguration<EndpointsConfiguration>(
            x => x.AddYamlFile("configuration.yml").AddCommandLineArgs()
        );
        container.AddNetMQMessageBus(
            (sp, opts) =>
                opts.WithSerializer(sp.Resolve<ISerializer<string>>())
                    .WithEndpoints(sp.Resolve<EndpointsConfiguration>())
        );
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
