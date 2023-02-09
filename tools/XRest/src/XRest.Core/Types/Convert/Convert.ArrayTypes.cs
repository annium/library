using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using XRest.Core.Types.Models;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static readonly Type BaseArrayType = typeof(IEnumerable<>);
    private static readonly HashSet<Type> ArrayTypes = new();

    private static void RegisterBasicArrayTypes()
    {
        RegisterArrayType(typeof(IEnumerable<>));
        RegisterArrayType(typeof(IReadOnlyCollection<>));
        RegisterArrayType(typeof(ICollection<>));
        RegisterArrayType(typeof(IReadOnlyList<>));
        RegisterArrayType(typeof(IList<>));
        RegisterArrayType(typeof(IReadOnlySet<>));
        RegisterArrayType(typeof(ISet<>));
        RegisterArrayType(typeof(List<>));
        RegisterArrayType(typeof(HashSet<>));
    }

    public static void RegisterArrayType(Type type)
    {
        if (type.IsGenericParameter)
            throw new ArgumentException($"Can't register generic parameter {type.FriendlyName()} as array type");

        if (type is { IsGenericType: true, IsGenericTypeDefinition: false })
            throw new ArgumentException($"Can't register generic type {type.FriendlyName()} as array type");

        if (type != BaseArrayType && !type.IsDerivedFrom(BaseArrayType))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseArrayType.FriendlyName()}");

        if (!ArrayTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as array type");
    }

    private static bool IsArrayType(Type type) => type.IsArray || ArrayTypes.Contains(type.IsGenericType ? type.GetGenericTypeDefinition() : type);

    private static ITypeModel ResolveArrayTypeModel(Type type)
    {
        if (!IsArrayType(type))
            throw new ArgumentException($"Can't get non array type {type.FriendlyName()} element type");

        var elementType = type.IsArray
            ? type.GetElementType() ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}")
            : type.GetTargetImplementation(BaseArrayType)!.GetGenericArguments()[0];
        var elementTypeModel = GetTypeModel(elementType);

        return new ArrayModel(elementTypeModel);
    }
}