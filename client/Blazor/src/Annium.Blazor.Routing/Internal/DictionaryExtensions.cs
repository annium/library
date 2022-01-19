using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;

namespace Annium.Blazor.Routing.Internal;

internal static class DictionaryExtensions
{
    public static Dictionary<string, PropertyInfo> ToPropertiesDictionary(
        this IEnumerable<PropertyInfo> properties
    ) => properties.ToDictionary(x => x.Name.CamelCase(), x => x);
}