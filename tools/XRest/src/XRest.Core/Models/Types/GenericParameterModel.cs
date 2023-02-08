namespace XRest.Core.Models.Types;

public sealed record GenericParameterModel(string Name) : ITypeModel
{
    public override string ToString() => $"Generic parameter {Name}";
    public bool IsGeneric => true;
}