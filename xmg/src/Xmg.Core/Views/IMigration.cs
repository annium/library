using System.Collections.Generic;

namespace Xmg.Core.Views;

public interface IMigration
{
    IReadOnlyDictionary<string, string> Files { get; }
}
