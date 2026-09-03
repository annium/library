using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides extension methods for creating dictionaries from property collections.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Converts a collection of PropertyInfo objects to a dictionary with camelCase property names as keys.
    /// </summary>
    /// <param name="properties">The collection of properties to convert.</param>
    /// <returns>A dictionary with camelCase property names as keys and PropertyInfo objects as values.</returns>
    public static Dictionary<string, PropertyInfo> ToPropertiesDictionary(this IEnumerable<PropertyInfo> properties) =>
        properties.ToDictionary(x => x.Name.CamelCase(), x => x);
}
