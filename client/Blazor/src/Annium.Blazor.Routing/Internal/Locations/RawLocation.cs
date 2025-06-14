using System.Collections.Generic;
using Annium.Net.Base;
using Microsoft.Extensions.Primitives;

namespace Annium.Blazor.Routing.Internal.Locations;

/// <summary>
/// Represents a raw location parsed from a URI, containing path segments and query parameters
/// </summary>
internal sealed record RawLocation
{
    /// <summary>
    /// Parses a URI string into a RawLocation instance
    /// </summary>
    /// <param name="uri">The URI string to parse</param>
    /// <returns>A RawLocation instance containing the parsed segments and parameters</returns>
    public static RawLocation Parse(string uri)
    {
        if (!uri.Contains('?'))
            return new RawLocation(Helper.ParseTemplateParts(uri), new Dictionary<string, StringValues>());

        var (path, rawQuery, _) = uri.Split('?');
        var query = UriQuery.Parse(rawQuery);

        return new RawLocation(Helper.ParseTemplateParts(path), query);
    }

    /// <summary>
    /// Gets the path segments extracted from the URI
    /// </summary>
    public IReadOnlyList<string> Segments { get; }

    /// <summary>
    /// Gets the query parameters extracted from the URI
    /// </summary>
    public IReadOnlyDictionary<string, StringValues> Parameters { get; }

    /// <summary>
    /// Initializes a new instance of the RawLocation record
    /// </summary>
    /// <param name="segments">The path segments</param>
    /// <param name="parameters">The query parameters</param>
    private RawLocation(IReadOnlyList<string> segments, IReadOnlyDictionary<string, StringValues> parameters)
    {
        Segments = segments;
        Parameters = parameters;
    }
}
