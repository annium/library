using System.Collections.Generic;

namespace Xc.Setup.Raw;

internal class RootSetup
{
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, string> Includes { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, Dictionary<string, TargetSetup>> Targets { get; set; } =
        new Dictionary<string, Dictionary<string, TargetSetup>>();
}