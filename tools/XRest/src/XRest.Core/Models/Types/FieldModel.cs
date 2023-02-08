namespace XRest.Core.Models.Types;

public sealed record FieldModel(
    ITypeModel Type,
    string Name
)
{
    public override string ToString() => $"{Type} {Name}";
}