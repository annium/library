using Annium.Net.Types.Refs;

namespace Annium.XRest.Core.Models;

public sealed record ParameterModel(ParameterLocationEnum Location, IRef Type, string Name)
{
    public override string ToString() => $"[{Location}] {Type} {Name}";
}
