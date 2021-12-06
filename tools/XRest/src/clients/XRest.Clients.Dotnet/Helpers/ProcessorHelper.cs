using System;
using System.Collections.Generic;

namespace XRest.Clients.Dotnet.Helpers;

internal static class ProcessorHelper
{
    private static readonly IReadOnlyDictionary<Type, string> BaseTypeNames = new Dictionary<Type, string>
    {
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(bool), "bool" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(char), "char" },
        { typeof(string), "string" },
        { typeof(object), "object" },
        { typeof(void), "void" },
    };

    public static bool IsBaseType(Type type) => BaseTypeNames.ContainsKey(type);
}