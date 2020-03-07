using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NodaTime;

namespace xrest.Tools
{
    public static class TypeMap
    {
        public static readonly ImmutableDictionary<Type, BaseType> BaseTypes = new Dictionary<Type, BaseType>()
        {
            { typeof(bool), BaseType.Boolean },
            { typeof(byte), BaseType.Number },
            { typeof(sbyte), BaseType.Number },
            { typeof(char), BaseType.String },
            { typeof(decimal), BaseType.Number },
            { typeof(double), BaseType.Number },
            { typeof(float), BaseType.Number },
            { typeof(int), BaseType.Number },
            { typeof(uint), BaseType.Number },
            { typeof(long), BaseType.Number },
            { typeof(ulong), BaseType.Number },
            { typeof(short), BaseType.Number },
            { typeof(ushort), BaseType.Number },
            { typeof(string), BaseType.String },
            { typeof(DateTimeOffset), BaseType.Date },
            { typeof(DateTime), BaseType.Date },
            { typeof(Instant), BaseType.Date },
            { typeof(Guid), BaseType.String },
        }.ToImmutableDictionary();
    }
}