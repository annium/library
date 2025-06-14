using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Annium.Blazor.Routing.Internal.Locations.Segments;
using Annium.Core.Mapper;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Represents a location path that can match URL segments and generate links based on route templates
/// </summary>
internal class LocationPath : ILocationPath
{
    /// <summary>
    /// Regular expression for matching parameter placeholders in route templates
    /// </summary>
    private static readonly Regex _paramRe = new(@"^\{([A-z0-9]+)\}$", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Parses a route template string and creates a LocationPath instance with associated property information
    /// </summary>
    /// <param name="template">The route template string containing path segments and parameter placeholders</param>
    /// <param name="properties">Collection of properties that can be used as route parameters</param>
    /// <param name="mapper">Mapper instance for type conversions</param>
    /// <returns>A tuple containing the created LocationPath and the collection of properties used as path parameters</returns>
    public static (ILocationPath, IReadOnlyCollection<PropertyInfo>) Parse(
        string template,
        IReadOnlyCollection<PropertyInfo> properties,
        IMapper mapper
    )
    {
        var propertiesDictionary = properties.ToPropertiesDictionary();
        var pathProperties = new Dictionary<string, PropertyInfo>();
        var segments = Helper
            .ParseTemplateParts(template)
            .Select<string, ILocationSegment>(x =>
            {
                var match = _paramRe.Match(x);
                if (!match.Success)
                    return new FixedLocationSegment(x);

                var name = match.Groups[1].Value;
                if (!propertiesDictionary.TryGetValue(name, out var property))
                    throw new ArgumentException($"Path template '{template}' contains unknown parameter '{name}'");

                pathProperties[name] = property;

                return new ParamLocationSegment(name, property.PropertyType);
            })
            .ToArray();

        return (new LocationPath(segments, mapper), pathProperties.Values);
    }

    /// <summary>
    /// The collection of location segments that make up this path
    /// </summary>
    private readonly IReadOnlyList<ILocationSegment> _segments;

    /// <summary>
    /// Mapper instance used for type conversions during matching and link generation
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the LocationPath class
    /// </summary>
    /// <param name="segments">The collection of location segments</param>
    /// <param name="mapper">Mapper instance for type conversions</param>
    private LocationPath(IReadOnlyList<ILocationSegment> segments, IMapper mapper)
    {
        _segments = segments;
        _mapper = mapper;
    }

    /// <summary>
    /// Attempts to match the provided URL segments against this location path
    /// </summary>
    /// <param name="segments">The URL segments to match</param>
    /// <param name="match">The type of path matching to perform</param>
    /// <returns>A LocationMatch result indicating success or failure and extracted route values</returns>
    public LocationMatch Match(IReadOnlyList<string> segments, PathMatch match)
    {
        if (segments.Count > _segments.Count)
            return LocationMatch.Empty;

        if (match == PathMatch.Exact && segments.Count != _segments.Count)
            return LocationMatch.Empty;

        var routeValues = new Dictionary<string, object?>();
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = _segments[i];
            var raw = segments[i];

            switch (segment)
            {
                case FixedLocationSegment fixedSegment:
                    if (!fixedSegment.Match(raw))
                        return LocationMatch.Empty;
                    break;
                case ParamLocationSegment paramSegment:
                    var value = paramSegment.Match(raw, _mapper);
                    if (value is null)
                        return LocationMatch.Empty;
                    routeValues[paramSegment.Name] = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return new LocationMatch(true, routeValues);
    }

    /// <summary>
    /// Generates a URL path string using the provided parameters
    /// </summary>
    /// <param name="parameters">Dictionary of parameter values to substitute into the path template</param>
    /// <returns>The generated URL path string</returns>
    public string Link(IReadOnlyDictionary<string, object?> parameters) =>
        string.Join(
            Constants.Separator,
            _segments.Select(x =>
            {
                if (x is FixedLocationSegment fs)
                    return fs.Part;
                if (x is ParamLocationSegment ps)
                    if (parameters.TryGetValue(ps.Name, out var value))
                        return _mapper.Map<string>(value);
                    else
                        throw new ArgumentException($"Path requires parameter '{ps.Name}'");

                throw new NotImplementedException($"Segment {x} is not supported");
            })
        );
}
