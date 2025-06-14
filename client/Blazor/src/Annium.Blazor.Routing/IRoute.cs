namespace Annium.Blazor.Routing;

/// <summary>
/// Represents a route without parameters that provides navigation and state checking functionality
/// </summary>
public interface IRoute
{
    /// <summary>
    /// Generates the URL link for this route
    /// </summary>
    /// <returns>The URL string for this route</returns>
    string Link();

    /// <summary>
    /// Navigates to this route
    /// </summary>
    void Go();

    /// <summary>
    /// Determines whether the current location matches this route
    /// </summary>
    /// <param name="match">The path matching strategy to use</param>
    /// <returns>True if the current location matches this route; otherwise, false</returns>
    bool IsAt(PathMatch match = PathMatch.Exact);
}
