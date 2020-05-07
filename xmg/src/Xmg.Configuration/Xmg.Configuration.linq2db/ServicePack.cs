using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xmg.Configuration.Abstractions.Components;
using Xmg.Configuration.linq2db.Components;

namespace Xmg.Configuration.linq2db
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Core.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // components
            services.AddSingleton<IConfigurator, Configurator>();
            services.AddSingleton<ILoader, Loader>();
            services.AddSingleton<IMetadataProcessor, MetadataProcessor>();
        }
    }
}