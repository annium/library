namespace Annium.Blazor.Routing;

/// <summary>
/// Factory for creating route instances with various parameter configurations
/// </summary>
public interface IRouteFactory
{
    /// <summary>
    /// Creates a parameterized route instance for the specified page type and data type
    /// </summary>
    /// <typeparam name="TPage">The page component type associated with this route</typeparam>
    /// <typeparam name="TData">The data parameter type for this route</typeparam>
    /// <param name="template">The URL template pattern for this route</param>
    /// <returns>A parameterized route instance</returns>
    IRoute<TData> Create<TPage, TData>(string template)
        where TPage : notnull
        where TData : notnull, new();

    /// <summary>
    /// Creates a parameter-less route instance for the specified page type
    /// </summary>
    /// <typeparam name="TPage">The page component type associated with this route</typeparam>
    /// <param name="template">The URL template pattern for this route</param>
    /// <returns>A parameter-less route instance</returns>
    IRoute Create<TPage>(string template)
        where TPage : notnull;
}
