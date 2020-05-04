using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xmg.Configuration.Components;

namespace Xmg.Configuration
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<linq2db.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // components
            services.AddSingleton<IConfiguratorFactory, ConfiguratorFactory>();
        }
    }
}