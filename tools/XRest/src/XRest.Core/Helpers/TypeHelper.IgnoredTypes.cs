using System;
using System.Collections.Generic;
using Annium.Core.Primitives;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static readonly HashSet<Type> IgnoredTypes = new();

    private static void RegisterBasicIgnoredTypes()
    {
        // basic types
        RegisterIgnoredType(typeof(object));
        RegisterIgnoredType(typeof(ValueType));
        // enumerable interfaces
        RegisterIgnoredType(typeof(IEnumerable<>));
        RegisterIgnoredType(typeof(ICollection<>));
        RegisterIgnoredType(typeof(IReadOnlyCollection<>));
        // dictionary interfaces
        RegisterIgnoredType(typeof(IReadOnlyDictionary<,>));
        RegisterIgnoredType(typeof(IDictionary<,>));
        // base type interfaces
        RegisterIgnoredType(typeof(IComparable<>));
        RegisterIgnoredType(typeof(IEquatable<>));
        // low-level interfaces
        RegisterIgnoredType(typeof(ISpanParsable<>));
        RegisterIgnoredType(typeof(IParsable<>));
    }

    public static void RegisterIgnoredType(Type type)
    {
        if (type.IsGenericParameter)
            throw new ArgumentException($"Can't register generic parameter {type.FriendlyName()} as ignored type");

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
            throw new ArgumentException($"Can't register generic type {type.FriendlyName()} as ignored type");

        if (!IgnoredTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already ignored");
    }

    private static bool IsIgnoredType(Type type) => IgnoredTypes.Contains(type.IsGenericType ? type.GetGenericTypeDefinition() : type);
}