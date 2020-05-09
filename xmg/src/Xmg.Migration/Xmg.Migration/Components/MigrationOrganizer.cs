using System.Collections.Generic;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.Components
{
    internal class MigrationOrganizer : IMigrationOrganizer
    {
        public IReadOnlyCollection<IOperation> OrganizeUp(IReadOnlyCollection<IOperation> operations)
        {
            var result = new List<IOperation>();

            result.AddRange(operations);

            return result;
        }

        public IReadOnlyCollection<IOperation> OrganizeDown(IReadOnlyCollection<IOperation> operations)
        {
            var result = new List<IOperation>();

            result.AddRange(operations);

            return result;
        }
    }
}