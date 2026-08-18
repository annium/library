namespace Annium.Blazor.Routing;

/// <summary>
/// How strictly a request path must line up with a route's template for the route to match.
/// </summary>
public enum PathMatch
{
    /// <summary>
    /// The whole path must equal the template.
    /// </summary>
    Exact = 0,

    /// <summary>
    /// The path must begin with the template, so nested paths match too.
    /// </summary>
    Start = 1,
}
