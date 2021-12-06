using System;
using Annium.Core.DependencyInjection;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.Components;

namespace Xmg.Migration;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<FluentMigrator.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // components
        container.Add<IMigratorFactory, MigratorFactory>().Singleton();
        container.Add<IMigrationOrganizer, MigrationOrganizer>().Singleton();
    }
}