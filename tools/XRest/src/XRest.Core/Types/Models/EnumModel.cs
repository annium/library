using System.Collections.Generic;
using XRest.Core.Models;

namespace XRest.Core.Types.Models;

public sealed record EnumModel(
    Namespace Namespace,
    string Name,
    IReadOnlyDictionary<string, long> Values
) : TypeModelBase(Namespace, Name, false)
{
    public override string ToString() => $"enum {Namespace}.{Name}";
}