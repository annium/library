using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using NodaTime;
using XRest.Core.Extensions;

namespace XRest.Core.Models.Types;

public static class BaseType
{
    private static readonly Dictionary<Type, StructModel> BaseTypes = new();

    static BaseType()
    {
        Register<bool>("bool");
        Register<string>("string");
        Register<byte>("byte");
        Register<sbyte>("sbyte");
        Register<int>("int");
        Register<uint>("uint");
        Register<long>("long");
        Register<ulong>("ulong");
        Register<DateTime>("datetime");
        Register<DateOnly>("date");
        Register<TimeOnly>("time");
        Register<Instant>("instant");
        Register<Duration>("duration");
        Register(typeof(void), "void");
    }

    public static StructModel? GetFor(Type type) => BaseTypes.GetValueOrDefault(type);

    public static void Register<T>(string name) => Register(typeof(T), name);

    public static void Register(Type type, string name)
    {
        if (type is { IsClass: false, IsValueType: false })
            throw new ArgumentException($"Type {type.FriendlyName()} is neither class nor struct");

        if (type.IsGenericType || type.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type");

        if (type.IsGenericTypeParameter)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type parameter");

        if (!BaseTypes.TryAdd(type, StructModelBuilder.Init(type.GetNamespace(), name).Build()))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered");
    }
}