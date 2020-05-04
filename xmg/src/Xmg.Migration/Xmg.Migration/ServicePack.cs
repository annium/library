using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xmg.Migration.Components;

namespace Xmg.Migration
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Migration.FluentMigrator.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // components
            services.AddSingleton<IMigratorFactory, MigratorFactory>();
        }
    }
}