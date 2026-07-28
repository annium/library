namespace Annium.Blazor.Routing;

/// <summary>
/// Represents a parameterized route that provides navigation and state checking functionality with typed data
/// </summary>
/// <typeparam name="TData">The type of data parameters for this route</typeparam>
public interface IRoute<TData>
    where TData : notnull, new()
{
    /// <summary>
    /// Generates the URL link for this route with the specified data parameters
    /// </summary>
    /// <param name="data">The data parameters to include in the URL</param>
    /// <returns>The URL string for this route with the specified parameters</returns>
    string Link(TData data);

    /// <summary>
    /// Navigates to this route with the specified data parameters
    /// </summary>
    /// <param name="data">The data parameters for navigation</param>
    void Go(TData data);

    /// <summary>
    /// Determines whether the current location matches this route with optional data comparison
    /// </summary>
    /// <param name="data">The data parameters to compare against, or null to ignore parameter comparison</param>
    /// <param name="match">The path matching strategy to use</param>
    /// <returns>True if the current location matches this route and data; otherwise, false</returns>
    bool IsAt(TData? data = default, PathMatch match = PathMatch.Exact);

    /// <summary>
    /// Attempts to extract route parameters from the current location
    /// </summary>
    /// <param name="data">When this method returns, contains the extracted parameters if successful</param>
    /// <returns>True if parameters were successfully extracted; otherwise, false</returns>
    bool TryGetParams(out TData data);

    /// <summary>
    /// Extracts route parameters from the current location
    /// </summary>
    /// <returns>The extracted route parameters</returns>
    TData GetParams();

    /// <summary>
    /// Creates a parameter-less, PATH-ONLY route with this route's path parameters baked from the specified data.
    /// Query-only properties of <typeparamref name="TData"/> are not represented in the result (a non-generic
    /// route carries no query string) — use <see cref="Link(TData)"/> when the query is needed.
    /// </summary>
    /// <param name="data">The data whose path parameters are baked into the route</param>
    /// <returns>A parameter-less, path-only route instance with the path parameters pre-bound</returns>
    IRoute Bind(TData data);
}
