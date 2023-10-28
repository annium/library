using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;

namespace Xmg.Migration.Components;

public interface IMigratorFactory
{
    IMigrator GetForProvider(MigrationProvider provider);
}
