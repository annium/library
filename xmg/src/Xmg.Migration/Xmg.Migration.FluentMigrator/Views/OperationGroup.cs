using System.Collections.Generic;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class OperationGroup
    {
        public string Name { get; }
        public IReadOnlyCollection<IOperation> Operations { get; }

        public OperationGroup(
            string name,
            IReadOnlyCollection<IOperation> operations
        )
        {
            Name = name;
            Operations = operations;
        }

        public OperationGroup(
            string name,
            params IOperation[] operations
        )
        {
            Name = name;
            Operations = operations;
        }
    }
}