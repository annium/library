using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XRest.TypeScript.Helpers
{
    internal static class GenerateHelper
    {
        public static bool IsDictionary(Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return false;

            var definition = x.GetGenericTypeDefinition();

            return definition == typeof(Dictionary<,>) ||
                definition == typeof(IDictionary<,>) ||
                definition == typeof(ReadOnlyDictionary<,>) ||
                definition == typeof(IReadOnlyDictionary<,>);
        });

        public static bool IsArray(Type type) => type.GetInterfaces().Any(x =>
        {
            if (!x.IsGenericType)
                return false;

            var definition = x.GetGenericTypeDefinition();
            if (definition != typeof(IEnumerable<>))
                return false;

            return definition == typeof(IEnumerable<>) ||
                definition.IsArray ||
                definition == typeof(List<>) ||
                definition == typeof(LinkedList<>) ||
                definition == typeof(IList<>) ||
                definition == typeof(IReadOnlyList<>) ||
                definition == typeof(HashSet<>) ||
                definition == typeof(ISet<>) ||
                definition == typeof(Stack<>) ||
                definition == typeof(Collection<>) ||
                definition == typeof(ICollection<>) ||
                definition == typeof(ReadOnlyCollection<>) ||
                definition == typeof(IReadOnlyCollection<>);
        });
    }
}