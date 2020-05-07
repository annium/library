using System.Collections.Generic;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class MigrationOperationGroup : IMigrationOperation
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
    }
}