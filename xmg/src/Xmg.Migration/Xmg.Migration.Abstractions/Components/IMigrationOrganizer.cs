using System.Collections.Generic;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.Abstractions.Components
{
    public interface IMigrationOrganizer
    {
        IReadOnlyCollection<IOperation> OrganizeUp(IReadOnlyCollection<IOperation> operations);
        IReadOnlyCollection<IOperation> OrganizeDown(IReadOnlyCollection<IOperation> operations);
    }
}