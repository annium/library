using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Data.Operations;
using NodaTime;
using XRest.Clients.TypeScript.Helpers;

namespace XRest.Clients.TypeScript.Views.Types;

internal static class KnownTypes
{
    public static readonly TypeMap BuiltIn = new TypeMap()
        .Register(typeof(void), BaseType.Void)
        .Register(typeof(object), BaseType.Object)
        .Register(typeof(bool), BaseType.Boolean)
        .Register(typeof(byte), BaseType.Number)
        .Register(typeof(sbyte), BaseType.Number)
        .Register(typeof(char), BaseType.String)
        .Register(typeof(decimal), BaseType.Number)
        .Register(typeof(double), BaseType.Number)
        .Register(typeof(float), BaseType.Number)
        .Register(typeof(int), BaseType.Number)
        .Register(typeof(uint), BaseType.Number)
        .Register(typeof(long), BaseType.Number)
        .Register(typeof(ulong), BaseType.Number)
        .Register(typeof(short), BaseType.Number)
        .Register(typeof(ushort), BaseType.Number)
        .Register(typeof(string), BaseType.String)
        .Register(typeof(DateTimeOffset), BaseType.String)
        .Register(typeof(DateTime), BaseType.String)
        .Register(typeof(Instant), BaseType.String)
        .Register(typeof(Guid), BaseType.String)
        .Register(typeof(IDictionary), BaseType.Object)
        .Register(typeof(IDictionary<,>), BaseType.Record)
        .Register(typeof(IReadOnlyDictionary<,>), BaseType.Record)
        .Register(typeof(IEnumerable<>), BaseType.Array)
        .Register(typeof(Array), BaseType.Array)
        .Register(typeof(Nullable<>), BaseType.Nullable)
        .Register(typeof(IResult), ExternalType.HttpResponseVoid)
        .Register(typeof(IResult<>), ExternalType.HttpResponse);

    public static readonly TypeSet Skipped = new TypeSet()
        .Register(typeof(Task))
        .Register(typeof(Task<>))
        .Register(typeof(Nullable<>));
}
