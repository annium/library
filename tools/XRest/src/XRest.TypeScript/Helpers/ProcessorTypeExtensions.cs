using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Helpers
{
    internal static class ProcessorTypeExtensions
    {
        public static IReadOnlyCollection<PropertyInfo> GetAllPublicProperties(this Type type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(x => x.CanRead)
            .ToArray();

        public static bool IsSkipped(this Type type) => KnownTypes.Skipped.Contains(type) || type.IsDictionary() || type.IsArray();

        public static bool IsDictionary(this Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return x == typeof(IDictionary);

            var definition = x.GetGenericTypeDefinition();

            return definition == typeof(IDictionary<,>) ||
                definition == typeof(IReadOnlyDictionary<,>);
        });

        public static bool IsArray(this Type type) => type.IsArray || type.GetInterfaces()
            .Any(x => x.IsGenericType ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>) : x == typeof(IEnumerable));
    }
}