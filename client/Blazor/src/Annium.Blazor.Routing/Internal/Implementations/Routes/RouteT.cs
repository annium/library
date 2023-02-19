using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Routing.Internal.Implementations.Locations;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Linq;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Routing.Internal.Implementations.Routes;

internal class Route<TData> : RouteBase, IRoute<TData>
    where TData : notnull, new()
{
    private readonly TData _default = new();
    private readonly IReadOnlyDictionary<string, object?> _defaultParameters;
    private readonly IMapper _mapper;
    private readonly ILocationPath _path;
    private readonly ILocationQuery _query;
    private readonly IDataModel _model;
    private readonly IDataModel _pathModel;
    private readonly IDataModel _queryModel;

    public Route(
        NavigationManager navigationManager,
        string template,
        Type pageType,
        IMapper mapper
    ) : base(navigationManager, template, pageType)
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

    public string Link(TData data)
    {
        var pathParams = _pathModel.ToParams(data);
        var path = _path.Link(pathParams);
        var queryParams = _queryModel.ToParams(data);
        var query = _queryModel.ToQuery(queryParams);

        return path + query;
    }

    public void Go(TData data)
    {
        var link = Link(data);
        NavigationManager.NavigateTo(link);
    }

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
        var parameters = _model.ToParams(data)
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

    public TData GetParams()
    {
        if (!TryGetParams(out var data))
            throw new InvalidOperationException("Route doesn't match location");

        return data;
    }

    public override LocationMatch Match(RawLocation raw, PathMatch match)
    {
        var pathMatch = _path.Match(raw.Segments, match);
        if (!pathMatch.IsSuccess)
            return LocationMatch.Empty;

        var queryMatch = _query.Match(raw.Parameters);
        var values = queryMatch.RouteValues.Merge(pathMatch.RouteValues);

        return new LocationMatch(true, values);
    }

    public IRoute Bind(TData data)
    {
        var pathParams = _pathModel.ToParams(data);
        var path = _path.Link(pathParams);

        return new Route(NavigationManager, path, PageType, _mapper);
    }
}