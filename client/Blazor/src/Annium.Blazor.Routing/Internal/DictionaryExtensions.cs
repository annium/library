using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;

namespace Annium.Blazor.Routing.Internal
{
    internal static class DictionaryExtensions
    {
        public static Dictionary<string, PropertyInfo> ToPropertiesDictionary(
            this IEnumerable<PropertyInfo> properties
        ) => properties.ToDictionary(x => x.Name.CamelCase(), x => x);

        public static Dictionary<string, object> Merge(
            this IEnumerable<KeyValuePair<string, object>> source,
            IEnumerable<KeyValuePair<string, object>> target
        )
        {
            var values = source.ToDictionary(x => x.Key, x => x.Value);
            foreach (var (key, value) in target)
                values[key] = value;

            return values;
        }
    }
}