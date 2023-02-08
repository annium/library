using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives.Collections.Generic;

namespace XRest.Core.Models.Types;

public sealed record StructModel : TypeModelBase
{
    private static string ResolveName(string name, IReadOnlyCollection<ITypeModel> genericArguments) =>
        genericArguments.Count == 0
            ? name
            : $"{name}<{genericArguments.Select(x => x.Name).Join(", ")}>";

    private static bool ResolveIsGeneric(IReadOnlyCollection<ITypeModel> genericArguments) =>
        genericArguments.Any(x => x.IsGeneric);

    public StructModel(
        Namespace @namespace,
        string name,
        IReadOnlyList<ITypeModel> genericArguments
    ) : this(@namespace, name, genericArguments, Array.Empty<FieldModel>())
    {
    }

    public StructModel(
        Namespace @namespace,
        string name,
        IReadOnlyList<FieldModel> fields
    ) : this(@namespace, name, Array.Empty<StructModel>(), fields)
    {
    }

    public StructModel(
        Namespace @namespace,
        string name
    ) : this(@namespace, name, Array.Empty<StructModel>(), Array.Empty<FieldModel>())
    {
    }

    public StructModel(
        Namespace @namespace,
        string name,
        IReadOnlyList<ITypeModel> genericArguments,
        IReadOnlyList<FieldModel> fields
    ) : base(@namespace, ResolveName(name, genericArguments), ResolveIsGeneric(genericArguments))
    {
        GenericArguments = genericArguments;
        Fields = fields;
    }

    public override string ToString() => $"struct {Namespace}.{Name}";
    public IReadOnlyList<ITypeModel> GenericArguments { get; init; }
    public IReadOnlyList<FieldModel> Fields { get; init; }
}