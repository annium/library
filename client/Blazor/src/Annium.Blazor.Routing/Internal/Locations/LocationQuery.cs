using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Microsoft.Extensions.Primitives;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Represents a location query handler that can match URL query parameters against route properties
/// </summary>
internal class LocationQuery : ILocationQuery
{
    /// <summary>
    /// Creates a new LocationQuery instance with the specified properties and mapper
    /// </summary>
    /// <param name="properties">Collection of properties that can be matched against query parameters</param>
    /// <param name="mapper">Mapper instance for type conversions</param>
    /// <returns>A new LocationQuery instance</returns>
    public static ILocationQuery Create(IReadOnlyCollection<PropertyInfo> properties, IMapper mapper)
    {
        return new LocationQuery(properties.ToPropertiesDictionary(), mapper);
    }

    /// <summary>
    /// Dictionary of property names to PropertyInfo for quick lookup during matching
    /// </summary>
    private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;

    /// <summary>
    /// Mapper instance used for type conversions during query parameter matching
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the LocationQuery class
    /// </summary>
    /// <param name="properties">Dictionary of property names to PropertyInfo</param>
    /// <param name="mapper">Mapper instance for type conversions</param>
    private LocationQuery(IReadOnlyDictionary<string, PropertyInfo> properties, IMapper mapper)
    {
        _properties = properties;
        _mapper = mapper;
    }

    /// <summary>
    /// Attempts to match the provided query parameters against the configured properties
    /// </summary>
    /// <param name="query">Dictionary of query parameter names to their string values</param>
    /// <returns>A LocationMatch result with extracted and converted route values</returns>
    public LocationMatch Match(IReadOnlyDictionary<string, StringValues> query)
    {
        var routeValues = new Dictionary<string, object?>();

        foreach (var (key, raw) in query)
        {
            if (!_properties.TryGetValue(key, out var property))
                continue;

            var type = property.PropertyType;
            try
            {
                var value = type.IsEnumerable()
                    ? _mapper.Map(raw.ToArray(), type)
                    : _mapper.Map(raw.FirstOrDefault()!, type);
                routeValues[key] = value;
            }
            catch
            {
                // ignored
            }
        }

        return new LocationMatch(true, routeValues);
    }
}
