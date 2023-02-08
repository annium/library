using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;

namespace XRest.Core.Helpers;

public static partial class TypeHelper
{
    private static readonly Type BaseRecordType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>));
    private static readonly HashSet<Type> RecordTypes = new();

    private static void RegisterBasicRecordTypes()
    {
        RegisterRecordType(typeof(IDictionary<,>));
        RegisterRecordType(typeof(IReadOnlyDictionary<,>));
    }

    private static void RegisterRecordType(Type type)
    {
        if (type.IsGenericParameter)
            throw new ArgumentException($"Can't register generic parameter {type.FriendlyName()} as Record type");

        if (type is { IsGenericType: true, IsGenericTypeDefinition: false })
            throw new ArgumentException($"Can't register generic type {type.FriendlyName()} as Record type");

        if (type != BaseArrayType && !type.IsDerivedFrom(BaseArrayType))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseRecordType.FriendlyName()}");

        if (!RecordTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as Record type");
    }

    private static bool IsRecordType(Type type) => RecordTypes.Contains(type.IsGenericType ? type.GetGenericTypeDefinition() : type);

    private static (Type Key, Type Value) GetRecordTypeElement(Type type)
    {
        if (!IsRecordType(type))
            throw new ArgumentException($"Can't get non Record type {type.FriendlyName()} element type");

        var args = type.GetTargetImplementation(BaseArrayType)?.GetGenericArguments()[0].GetGenericArguments();

        return (args[0], args[1]);
    }
}