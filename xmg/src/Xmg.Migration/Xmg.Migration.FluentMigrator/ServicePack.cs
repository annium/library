using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.FluentMigrator.Components;

namespace Xmg.Migration.FluentMigrator
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
            services.AddSingleton<IMigrator, Migrator>();
        }
    }
}