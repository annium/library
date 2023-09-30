using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Routing.Internal;
using Annium.Blazor.Routing.Internal.Locations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Annium.Blazor.Routing;

public class Router : IComponent, IHandleAfterRender, IDisposable
{
    [Parameter]
    public RenderFragment NotFound { get; set; } = default!;

    [Parameter]
    public RenderFragment<RouteData> Found { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public INavigationInterception NavigationInterception { get; set; } = default!;

    [Inject]
    internal RouteManager RouteManager { get; set; } = default!;

    [Inject]
    internal IEnumerable<IRouting> Routings { get; set; } = Array.Empty<IRouting>();

    private RenderHandle _renderHandle;
    private bool _navigationInterceptionEnabled;
    private string _location = string.Empty;

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
        _location = NavigationManager.Uri;
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (Found is null)
            throw new InvalidOperationException($"The {nameof(Router)} component requires a value for the parameter {nameof(Found)}.");

        if (NotFound is null)
            throw new InvalidOperationException($"The {nameof(Router)} component requires a value for the parameter {nameof(NotFound)}.");

        Refresh();

        return Task.CompletedTask;
    }

    public Task OnAfterRenderAsync()
    {
        if (!_navigationInterceptionEnabled)
        {
            _navigationInterceptionEnabled = true;
            return NavigationInterception.EnableNavigationInterceptionAsync();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _location = args.Location;
        Refresh();
    }

    private void Refresh()
    {
        var rawLocation = RawLocation.Parse(NavigationManager.ToBaseRelativePath(_location));
        var locationData = RouteManager.Match(rawLocation, PathMatch.Exact) ?? RouteManager.Match(rawLocation, PathMatch.Start);

        var fragment = locationData is null ? NotFound : Found(new RouteData(locationData.PageType, locationData.RouteValues!));
        _renderHandle.Render(fragment);
    }
}