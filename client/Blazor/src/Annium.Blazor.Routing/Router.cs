using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Blazor.Routing.Internal;
using Annium.Blazor.Routing.Internal.Locations;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Annium.Blazor.Routing;

/// <summary>
/// A Blazor component that handles routing and navigation for single-page applications.
/// </summary>
public class Router : IComponent, IHandleAfterRender, IDisposable, ILogSubject
{
    /// <summary>
    /// Gets or sets the render fragment to display when no route matches.
    /// </summary>
    [Parameter]
    public RenderFragment NotFound { get; set; } = null!;

    /// <summary>
    /// Gets or sets the render fragment to display when a route matches.
    /// </summary>
    [Parameter]
    public RenderFragment<RouteData> Found { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation manager for handling navigation operations.
    /// </summary>
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation interception service for handling browser navigation events.
    /// </summary>
    [Inject]
    public INavigationInterception NavigationInterception { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets or sets the route manager for handling route matching.
    /// </summary>
    [Inject]
    internal RouteManager RouteManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of routing configurations.
    /// </summary>
    [Inject]
    internal IEnumerable<IRouting> Routings { get; set; } = [];

    /// <summary>
    /// The render handle for this component.
    /// </summary>
    private RenderHandle _renderHandle;

    /// <summary>
    /// Indicates whether navigation interception has been enabled.
    /// </summary>
    private bool _navigationInterceptionEnabled;

    /// <summary>
    /// The current location URI.
    /// </summary>
    private string _location = string.Empty;

    /// <summary>
    /// Attaches the component to the render tree and initializes navigation handling.
    /// </summary>
    /// <param name="renderHandle">The render handle for this component.</param>
    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
        _location = NavigationManager.Uri;
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    /// <summary>
    /// Sets the component parameters and validates required parameters.
    /// </summary>
    /// <param name="parameters">The parameter view containing component parameters.</param>
    /// <returns>A completed task.</returns>
    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (Found is null)
            throw new InvalidOperationException(
                $"The {nameof(Router)} component requires a value for the parameter {nameof(Found)}."
            );

        if (NotFound is null)
            throw new InvalidOperationException(
                $"The {nameof(Router)} component requires a value for the parameter {nameof(NotFound)}."
            );

        Refresh();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles post-render operations, specifically enabling navigation interception.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public Task OnAfterRenderAsync()
    {
        if (!_navigationInterceptionEnabled)
        {
            _navigationInterceptionEnabled = true;
            return NavigationInterception.EnableNavigationInterceptionAsync();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the component and cleans up event handlers.
    /// </summary>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }

    /// <summary>
    /// Handles location changed events from the navigation manager.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The location changed event arguments.</param>
    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _location = args.Location;
        Refresh();
    }

    /// <summary>
    /// Refreshes the component by matching the current location to routes and rendering the appropriate content.
    /// </summary>
    private void Refresh()
    {
        this.Trace("start");

        this.Trace<string>("parse location: {location}", _location);
        var rawLocation = RawLocation.Parse(NavigationManager.ToBaseRelativePath(_location));

        this.Trace<string>("find location for {location}", JsonSerializer.Serialize(rawLocation));
        // var locationData = RouteManager.Match(rawLocation, PathMatch.Exact) ?? RouteManager.Match(rawLocation, PathMatch.Start);
        var locationData = RouteManager.Match(rawLocation, PathMatch.Exact);

        if (locationData is null)
        {
            this.Trace("render not found");
            _renderHandle.Render(NotFound);
        }
        else
        {
            this.Trace<string?, string>(
                "render location of {pageType} with {values}",
                locationData.PageType.FullName,
                JsonSerializer.Serialize(locationData.RouteValues)
            );
            var fragment = Found(new RouteData(locationData.PageType, locationData.RouteValues!));
            _renderHandle.Render(fragment);
        }
    }
}
