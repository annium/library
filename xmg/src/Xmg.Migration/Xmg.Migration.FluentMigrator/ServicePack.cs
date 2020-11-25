using System;
using Annium.Core.DependencyInjection;
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

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            // components
            container.Add<IMigrator, Migrator>().Singleton();
        }
    }
}