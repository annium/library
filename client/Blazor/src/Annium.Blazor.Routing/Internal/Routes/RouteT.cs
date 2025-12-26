using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Linq;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Routes;

/// <summary>
/// Route implementation for pages with typed parameters.
/// </summary>
/// <typeparam name="TData">The type of data this route handles.</typeparam>
internal class Route<TData> : RouteBase, IRoute<TData>
    where TData : notnull, new()
{
    /// <summary>
    /// Default instance of the data type for parameter comparison.
    /// </summary>
    private readonly TData _default = new();

    /// <summary>
    /// Default parameter values for comparison.
    /// </summary>
    private readonly IReadOnlyDictionary<string, object?> _defaultParameters;

    /// <summary>
    /// Mapper for data transformations.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Location path handler for this route.
    /// </summary>
    private readonly ILocationPath _path;

    /// <summary>
    /// Location query handler for this route.
    /// </summary>
    private readonly ILocationQuery _query;

    /// <summary>
    /// Data model for the complete data type.
    /// </summary>
    private readonly IDataModel _model;

    /// <summary>
    /// Data model for path-related properties.
    /// </summary>
    private readonly IDataModel _pathModel;

    /// <summary>
    /// Data model for query-related properties.
    /// </summary>
    private readonly IDataModel _queryModel;

    /// <summary>
    /// Initializes a new instance of the Route class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager for navigation operations.</param>
    /// <param name="template">The route template pattern.</param>
    /// <param name="pageType">The page component type associated with this route.</param>
    /// <param name="mapper">The mapper for data transformations.</param>
    public Route(NavigationManager navigationManager, string template, Type pageType, IMapper mapper)
        : base(navigationManager, template, pageType)
    {
        var properties = DataModel.ResolveProperties<TData>();
        _mapper = mapper;
        var (path, pathProperties) = LocationPath.Parse(template, properties, mapper);
        var queryProperties = properties.Except(pathProperties).ToArray();
        _path = path;
        _query = LocationQuery.Create(queryProperties, mapper);
        _model = DataModel.Create<TData>(properties, mapper);
        _pathModel = DataModel.Create<TData>(pathProperties, mapper);
        _queryModel = DataModel.Create<TData>(queryProperties, mapper);
        _defaultParameters = _model.ToParams(_default);
    }

    /// <summary>
    /// Generates a link URL for this route with the specified data.
    /// </summary>
    /// <param name="data">The data to use for generating the link.</param>
    /// <returns>The generated link URL.</returns>
    public string Link(TData data)
    {
        var pathParams = _pathModel.ToParams(data);
        var path = _path.Link(pathParams);
        var queryParams = _queryModel.ToParams(data);
        var query = _queryModel.ToQuery(queryParams);

        return path + query;
    }

    /// <summary>
    /// Navigates to this route with the specified data.
    /// </summary>
    /// <param name="data">The data to use for navigation.</param>
    public void Go(TData data)
    {
        var link = Link(data);
        NavigationManager.NavigateTo(link);
    }

    /// <summary>
    /// Determines whether the current location matches this route with the specified data.
    /// </summary>
    /// <param name="data">The data to compare against the current location.</param>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>True if the current location matches this route with the specified data; otherwise, false.</returns>
    public bool IsAt(TData? data = default, PathMatch match = PathMatch.Exact)
    {
        var raw = RawLocation.Parse(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

        // ensure location is matched
        var (isSuccess, values) = Match(raw, match);
        if (!isSuccess)
            return false;

        // if data is default - default match is default
        if (data is null || data.Equals(_default))
            return true;

        // get parameters with non-default values
        var parameters = _model
            .ToParams(data)
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            .Where(x => x.Value is not null && !x.Value.Equals(_defaultParameters[x.Key]))
            .ToDictionary(x => x.Key, x => x.Value);

        // ensure all parameter values are equal to match values
        foreach (var (paramName, paramValue) in parameters)
            // if value doesn't exist, or is
            if (!values.TryGetValue(paramName, out var value) || !value.IsShallowEqual(paramValue, _mapper))
                return false;

        return true;
    }

    /// <summary>
    /// Attempts to extract route parameters from the current location.
    /// </summary>
    /// <param name="data">When this method returns, contains the extracted parameters if successful; otherwise, the default value.</param>
    /// <returns>True if parameters were successfully extracted; otherwise, false.</returns>
    public bool TryGetParams(out TData data)
    {
        var raw = RawLocation.Parse(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        data = default!;

        // ensure location is matched
        var (isSuccess, values) = Match(raw, PathMatch.Exact);
        if (!isSuccess)
            return false;

        // map values to object
        data = _model.ToData<TData>(values);

        return true;
    }

    /// <summary>
    /// Extracts route parameters from the current location.
    /// </summary>
    /// <returns>The extracted route parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the route doesn't match the current location.</exception>
    public TData GetParams()
    {
        if (!TryGetParams(out var data))
            throw new InvalidOperationException("Route doesn't match location");

        return data;
    }

    /// <summary>
    /// Attempts to match a raw location against this route.
    /// </summary>
    /// <param name="raw">The raw location to match against.</param>
    /// <param name="match">The path matching strategy to use.</param>
    /// <returns>A location match result indicating success and any extracted route values.</returns>
    public override LocationMatch Match(RawLocation raw, PathMatch match)
    {
        var pathMatch = _path.Match(raw.Segments, match);
        if (!pathMatch.IsSuccess)
            return LocationMatch.Empty;

        var queryMatch = _query.Match(raw.Parameters);
        var values = queryMatch.RouteValues.ToDictionary().Merge(pathMatch.RouteValues, MergeBehavior.KeepSource);

        return new LocationMatch(true, values);
    }

    /// <summary>
    /// Creates a non-generic route bound to the specified data values.
    /// </summary>
    /// <param name="data">The data to bind to the route.</param>
    /// <returns>A non-generic route with the data values bound.</returns>
    public IRoute Bind(TData data)
    {
        var pathParams = _pathModel.ToParams(data);
        var path = _path.Link(pathParams);

        return new Route(NavigationManager, path, PageType, _mapper);
    }
}
