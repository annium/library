using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XRest.Dotnet.Helpers
{
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

        public static IReadOnlyCollection<string> BuildUsages(string ns, IEnumerable<string> references)
        {
            return references
                .Where(x => !ns.Contains(x))
                .Distinct()
                .OrderBy(x => !x.StartsWith("System")).ThenBy(x => x)
                .ToArray();
        }

        public static string BuildPath(params string?[] parts) => Path.Combine(parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToArray());

        public static string BuildNamespace(params string?[] parts) => string.Join('.', parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}