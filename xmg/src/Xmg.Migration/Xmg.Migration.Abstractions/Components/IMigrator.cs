using Xmg.Core.Models;
using Xmg.Core.Views;

namespace Xmg.Migration.Abstractions.Components;

public interface IMigrator
{
    MigrationProvider Provider { get; }
    IMigration CreateMigration(Database database, Config cfg);
}
