using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRest.TypeScript.Helpers
{
    internal static class ProcessorHelper
    {
        public static bool IsDictionary(Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return x == typeof(IDictionary);

            var definition = x.GetGenericTypeDefinition();

            return definition == typeof(IDictionary<,>) ||
                definition == typeof(IReadOnlyDictionary<,>);
        });

        public static bool IsArray(Type type) => type.GetInterfaces()
            .Any(x => x.IsGenericType ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>) : x == typeof(IEnumerable));

        public static IReadOnlyCollection<PropertyInfo> GetProperties(Type type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(x => x.CanRead)
            .ToArray();
    }
}