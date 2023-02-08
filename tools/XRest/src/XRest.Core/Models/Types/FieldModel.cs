namespace XRest.Core.Models.Types;

public sealed record FieldModel(
    ITypeModel Type,
    string Name
)
{
}