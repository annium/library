using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Loader;
using Xmg.Configuration.Components;

namespace Xmg.Configuration;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<linq2db.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<IConfiguratorFactory, ConfiguratorFactory>().Singleton();

        container.AddAssemblyLoader();
    }
}
