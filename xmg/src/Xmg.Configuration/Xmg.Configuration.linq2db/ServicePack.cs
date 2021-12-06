using System;
using Annium.Core.DependencyInjection;
using Xmg.Configuration.Abstractions.Components;
using Xmg.Configuration.linq2db.Components;

namespace Xmg.Configuration.linq2db;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Core.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<IConfigurator, Configurator>().Singleton();
        container.Add<ILoader, Loader>().Singleton();
        container.Add<IMetadataProcessor, MetadataProcessor>().Singleton();
    }
}