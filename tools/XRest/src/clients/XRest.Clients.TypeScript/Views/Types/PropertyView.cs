using Annium.Core.Primitives;

namespace XRest.Clients.TypeScript.Views.Types;

internal class TypePropertyView
{
    public string Name { get; }
    public TypeView Type { get; }
    public bool IsOptional { get; }

    public TypePropertyView(
        string name,
        TypeView type,
        bool isOptional
    )
    {
        Name = name.CamelCase();
        Type = type;
        IsOptional = isOptional;
    }

    public override string ToString() => $"{Type} {Name}";
}