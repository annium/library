using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Core.Mapper;
using Microsoft.Extensions.Primitives;

namespace Annium.Blazor.Routing.Internal.Implementations.Locations;

internal class LocationQuery : ILocationQuery
{
    public static ILocationQuery Create(
        IReadOnlyCollection<PropertyInfo> properties,
        IMapper mapper
    )
    {
        return new LocationQuery(properties.ToPropertiesDictionary(), mapper);
    }

    private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;
    private readonly IMapper _mapper;

    private LocationQuery(
        IReadOnlyDictionary<string, PropertyInfo> properties,
        IMapper mapper
    )
    {
        _properties = properties;
        _mapper = mapper;
    }

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
                var value = type.IsEnumerable() ? _mapper.Map(raw.ToArray(), type) : _mapper.Map(raw.FirstOrDefault()!, type);
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