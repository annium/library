using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Represents the main chart context providing chart configuration, state management, and event handling
/// </summary>
public interface IChartContext
{
    /// <summary>
    /// Event triggered when lookup information changes
    /// </summary>
    event Action<Instant?, Point?> LookupChanged;

    /// <summary>
    /// Event triggered when the chart context is updated
    /// </summary>
    event Action Updated;

    /// <summary>
    /// Gets the current moment in time displayed on the chart
    /// </summary>
    Instant Moment { get; }

    /// <summary>
    /// Gets the current zoom level
    /// </summary>
    int Zoom { get; }

    /// <summary>
    /// Gets the available zoom levels
    /// </summary>
    IReadOnlyList<int> Zooms { get; }

    /// <summary>
    /// Gets the current time resolution
    /// </summary>
    Duration Resolution { get; }

    /// <summary>
    /// Gets the number of pixels per resolution unit
    /// </summary>
    int PxPerResolution { get; }

    /// <summary>
    /// Gets the available time resolutions
    /// </summary>
    IReadOnlyList<Duration> Resolutions { get; }

    /// <summary>
    /// Gets a value indicating whether the chart is locked
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    /// Gets the number of milliseconds per pixel
    /// </summary>
    int MsPerPx { get; }

    /// <summary>
    /// Gets the time zone used for chart display
    /// </summary>
    DateTimeZone TimeZone { get; }

    /// <summary>
    /// Gets the time zone offset in minutes
    /// </summary>
    int TimeZoneOffset { get; }

    /// <summary>
    /// Gets the time bounds of the chart data
    /// </summary>
    ValueRange<Instant> Bounds { get; }

    /// <summary>
    /// Gets the currently visible time range
    /// </summary>
    ValueRange<Instant> View { get; }

    /// <summary>
    /// Gets the collection of pane contexts
    /// </summary>
    IReadOnlyCollection<IPaneContext> Panes { get; }

    /// <summary>
    /// Configures the chart with available zoom levels and resolutions
    /// </summary>
    /// <param name="zooms">The available zoom levels</param>
    /// <param name="resolutions">The available time resolutions</param>
    void Configure(IReadOnlyList<int> zooms, IReadOnlyList<int> resolutions);

    /// <summary>
    /// Sets the current moment in time
    /// </summary>
    /// <param name="moment">The moment to set</param>
    void SetMoment(Instant moment);

    /// <summary>
    /// Sets the zoom level
    /// </summary>
    /// <param name="zoom">The zoom level to set</param>
    void SetZoom(int zoom);

    /// <summary>
    /// Sets the time resolution
    /// </summary>
    /// <param name="resolution">The resolution to set</param>
    void SetResolution(Duration resolution);

    /// <summary>
    /// Registers a pane context with the chart
    /// </summary>
    /// <param name="pane">The pane context to register</param>
    /// <returns>An action to unregister the pane</returns>
    Action RegisterPane(IPaneContext pane);

    /// <summary>
    /// Requests a redraw of the chart
    /// </summary>
    void RequestDraw();

    /// <summary>
    /// Requests an overlay at the specified point
    /// </summary>
    /// <param name="point">The point for the overlay, or null to hide overlay</param>
    void RequestOverlay(Point? point);
}
