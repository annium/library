using System.Collections.Generic;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class MigrationOperationGroup
    {
        public string Name { get; }
        public IReadOnlyCollection<IMigrationOperation> Operations { get; }

        public MigrationOperationGroup(
            string name,
            IReadOnlyCollection<IMigrationOperation> operations
        )
        {
            Name = name;
            Operations = operations;
        }

        public MigrationOperationGroup(
            string name,
            params IMigrationOperation[] operations
        )
        {
            Name = name;
            Operations = operations;
        }
    }
}