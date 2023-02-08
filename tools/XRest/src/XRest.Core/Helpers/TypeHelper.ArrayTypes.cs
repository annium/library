using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static readonly Type BaseArrayType = typeof(IEnumerable<>);
    private static readonly HashSet<Type> ArrayTypes = new();

    private static void RegisterBasicArrayTypes()
    {
        RegisterArrayType(typeof(IEnumerable<>));
        RegisterArrayType(typeof(ICollection<>));
        RegisterArrayType(typeof(IReadOnlyCollection<>));
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

    private static Type GetArrayTypeElement(Type type)
    {
        if (!IsArrayType(type))
            throw new ArgumentException($"Can't get non array type {type.FriendlyName()} element type");

        if (type.IsArray)
            return type.GetElementType()
                ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}");

        return type.GetTargetImplementation(BaseArrayType)!.GetGenericArguments()[0];
    }
}