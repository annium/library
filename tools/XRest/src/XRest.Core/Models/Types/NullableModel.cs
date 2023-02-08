namespace XRest.Core.Models.Types;

public record NullableModel(ITypeModel Type) : ITypeModel
{
    public string Name { get; } = $"{Type}?";
    public bool IsGeneric => Type.IsGeneric;
    public override string ToString() => Name;
}