using System;
using Annium.Core.Primitives;

namespace XRest.Core.Models;

public class ParameterModel
{
    public string Name { get; }
    public ParameterLocationEnum Location { get; }
    public Type Type { get; }

    public ParameterModel(
        string name,
        ParameterLocationEnum location,
        Type type
    )
    {
        Name = name;
        Location = location;
        Type = type;
    }

    public override string ToString() => $"[{Location}] {Type.FriendlyName()} {Name}";
}