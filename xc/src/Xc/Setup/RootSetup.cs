using System.Collections.Generic;

namespace Xc.Setup;

internal class RootSetup
{
    public string Source { get; }

    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TargetSetup>> Targets { get; }

    public RootSetup(string source, IReadOnlyDictionary<string, IReadOnlyDictionary<string, TargetSetup>> targets)
    {
        Source = source;
        Targets = targets;
    }
}
