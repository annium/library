using System;
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
        // drop any URL fragment (#...) first — it is not part of the path or query and must not fold into the
        // last path segment or the last query value (mirrors the framework Router, which strips '?' and '#')
        var hashIndex = uri.IndexOf('#');
        if (hashIndex >= 0)
            uri = uri[..hashIndex];

        if (!uri.Contains('?'))
            return new RawLocation(ParseSegments(uri), new Dictionary<string, StringValues>());

        var (path, rawQuery, _) = uri.Split('?');
        var query = UriQuery.Parse(rawQuery);

        return new RawLocation(ParseSegments(path), query);
    }

    /// <summary>
    /// Splits a live URL path into its non-empty segments. Unlike the strict template parser, empty segments
    /// produced by leading/trailing/double slashes are tolerated (dropped) rather than rejected — a real request
    /// URL like <c>statics/about/</c> must resolve, not crash routing.
    /// </summary>
    /// <param name="path">The URL path portion to split.</param>
    /// <returns>The non-empty path segments.</returns>
    private static IReadOnlyList<string> ParseSegments(string path) =>
        path.Split(Constants.Separator, StringSplitOptions.RemoveEmptyEntries);

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
