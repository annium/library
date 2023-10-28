using System.Collections.Generic;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.Abstractions.Components;

public interface IMigrationOrganizer
{
    IReadOnlyCollection<IOperation> Organize(IReadOnlyCollection<IOperation> operations);
}
