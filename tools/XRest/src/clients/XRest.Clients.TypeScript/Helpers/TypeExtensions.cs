using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Helpers
{
    internal static class ProcessorTypeExtensions
    {
        public static IReadOnlyCollection<PropertyInfo> GetAllPublicProperties(this Type type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(x => x.CanRead)
            .ToArray();

        public static bool IsSkipped(this Type type) =>
            KnownTypes.Skipped.Contains(type) || type.IsDictionary() || type.IsArray();

        public static bool IsDictionary(this Type type) =>
            type.IsDictionaryType() ||
            type.GetInterfaces().Any(IsDictionaryType);

        public static bool IsArray(this Type type)
        {
            if (type == typeof(string))
                return false;

            if (type.IsArray)
                return true;

            return type.GetInterfaces()
                .Any(x => x.IsGenericType
                    ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    : x == typeof(IEnumerable));
        }

        private static bool IsDictionaryType(this Type type)
        {
            if (!type.IsGenericType)
                return type == typeof(IDictionary);

            var definition = type.GetGenericTypeDefinition();

            return definition == typeof(IDictionary<,>) ||
                   definition == typeof(IReadOnlyDictionary<,>);
        }
    }
}