using XRest.Core.Models.Types;

namespace XRest.Core.Models;

public sealed record ParameterModel(
    ParameterLocationEnum Location,
    ITypeModel Type,
    string Name
)
{
    public override string ToString() => $"[{Location}] {Type} {Name}";
}