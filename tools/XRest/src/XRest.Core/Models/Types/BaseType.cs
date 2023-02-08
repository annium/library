using System;
using System.Collections.Generic;
using NodaTime;
using XRest.Core.Extensions;

namespace XRest.Core.Models.Types;

public static class BaseType
{
    private static readonly IReadOnlyDictionary<Type, StructModel> BaseTypes;

    static BaseType()
    {
        var baseTypes = new Dictionary<Type, StructModel>();
        Register<bool>("boolean");
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
        baseTypes[typeof(void)] = new StructModel(typeof(void).GetNamespace(), "void");
        BaseTypes = baseTypes;

        void Register<T>(string name) => baseTypes[typeof(T)] = new StructModel(typeof(T).GetNamespace(), name);
    }

    public static StructModel? GetFor(Type type) => BaseTypes.GetValueOrDefault(type);
}