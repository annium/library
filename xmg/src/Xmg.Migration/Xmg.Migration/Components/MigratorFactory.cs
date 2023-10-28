using System;
using System.Collections.Generic;
using System.Linq;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;

namespace Xmg.Migration.Components;

internal class MigratorFactory : IMigratorFactory
{
    private readonly IEnumerable<IMigrator> _migrators;

    public MigratorFactory(IEnumerable<IMigrator> migrators)
    {
        _migrators = migrators;
    }

    public IMigrator GetForProvider(MigrationProvider provider) =>
        _migrators.SingleOrDefault(x => x.Provider == provider)
        ?? throw new InvalidOperationException($"No migrator registered for provider {provider}");
}
