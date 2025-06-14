using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Data.Models;
using Annium.Logging;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

/// <summary>
/// Provides the main chart context that manages chart state, configuration, and coordinates interactions between panes and series.
/// </summary>
internal sealed record ChartContext : IManagedChartContext, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this chart context.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when the chart context is updated.
    /// </summary>
    public event Action Updated = delegate { };

    /// <summary>
    /// Event raised when the lookup position changes.
    /// </summary>
    public event Action<Instant?, Point?> LookupChanged = delegate { };

    /// <summary>
    /// Gets the current moment in time for the chart.
    /// </summary>
    public Instant Moment { get; private set; }

    /// <summary>
    /// Gets the current zoom level.
    /// </summary>
    public int Zoom { get; private set; }

    /// <summary>
    /// Gets the available zoom levels.
    /// </summary>
    public IReadOnlyList<int> Zooms => _zooms;

    /// <summary>
    /// Gets the current time resolution for the chart.
    /// </summary>
    public Duration Resolution { get; private set; }

    /// <summary>
    /// Gets the number of pixels per resolution unit.
    /// </summary>
    public int PxPerResolution { get; private set; }

    /// <summary>
    /// Gets the available time resolutions.
    /// </summary>
    public IReadOnlyList<Duration> Resolutions => _resolutions;

    /// <summary>
    /// Gets a value indicating whether the chart is locked (any pane is locked).
    /// </summary>
    public bool IsLocked => _panes.Any(x => x.IsLocked);

    /// <summary>
    /// Gets the number of milliseconds per pixel.
    /// </summary>
    public int MsPerPx { get; private set; }

    /// <summary>
    /// Gets the time zone used by the chart.
    /// </summary>
    public DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// Gets the time zone offset in minutes from UTC.
    /// </summary>
    public int TimeZoneOffset { get; } =
        DateTimeZoneProviders
            .Tzdb.GetSystemDefault()
            .GetUtcOffset(NodaConstants.UnixEpoch)
            .ToTimeSpan()
            .TotalMinutes.FloorInt32();

    /// <summary>
    /// Gets the time bounds of all data in the chart.
    /// </summary>
    public ValueRange<Instant> Bounds => _bounds;

    /// <summary>
    /// Gets the currently visible time range.
    /// </summary>
    public ValueRange<Instant> View => _view;

    /// <summary>
    /// Gets the collection of pane contexts registered with this chart.
    /// </summary>
    public IReadOnlyCollection<IPaneContext> Panes => _panes;

    /// <summary>
    /// Gets the DOM rectangle representing the chart's bounds.
    /// </summary>
    public DomRect Rect { get; private set; }

    /// <summary>
    /// The available zoom levels.
    /// </summary>
    private List<int> _zooms = [1];

    /// <summary>
    /// The available time resolutions.
    /// </summary>
    private List<Duration> _resolutions = [Duration.FromMinutes(1)];

    /// <summary>
    /// The collection of registered pane contexts.
    /// </summary>
    private readonly List<IPaneContext> _panes = [];

    /// <summary>
    /// The managed time bounds of all data in the chart.
    /// </summary>
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);

    /// <summary>
    /// The managed currently visible time range.
    /// </summary>
    private readonly ManagedValueRange<Instant> _view = ValueRange.Create(FutureBound, PastBound);

    /// <summary>
    /// Flag indicating whether the canvas needs to be redrawn.
    /// </summary>
    private int _isCanvasDirty = 1;

    /// <summary>
    /// The current overlay request state containing point and request flag.
    /// </summary>
    private (Point?, bool) _overlayRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartContext"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider for getting current time.</param>
    /// <param name="logger">The logger instance.</param>
    public ChartContext(ITimeProvider timeProvider, ILogger logger)
    {
        Logger = logger;
        Moment = timeProvider.Now;
        SetZoom(_zooms[0]);
        SetResolution(_resolutions[0]);
    }

    /// <summary>
    /// Sets the DOM rectangle bounds for the chart.
    /// </summary>
    /// <param name="rect">The DOM rectangle to set.</param>
    public void SetRect(DomRect rect)
    {
        Rect = rect;
        RequestDraw();
    }

    /// <summary>
    /// Configures the available zoom levels and time resolutions for the chart.
    /// </summary>
    /// <param name="zooms">The list of available zoom levels.</param>
    /// <param name="resolutions">The list of available time resolutions in minutes.</param>
    public void Configure(IReadOnlyList<int> zooms, IReadOnlyList<int> resolutions)
    {
        if (zooms.Count == 0)
            throw new ArgumentException("Zooms list is empty");

        if (resolutions.Count == 0)
            throw new ArgumentException("Resolutions list is empty");

        _zooms = zooms.ToList();
        SetZoom(_zooms[(_zooms.Count / (decimal)2).FloorInt32()]);
        _resolutions = resolutions.Select(i => Duration.FromMinutes(i)).ToList();
        SetResolution(_resolutions[0]);

        RequestDraw();
    }

    /// <summary>
    /// Registers a pane context with the chart.
    /// </summary>
    /// <param name="pane">The pane context to register.</param>
    /// <returns>An action that can be called to unregister the pane.</returns>
    public Action RegisterPane(IPaneContext pane)
    {
        if (_panes.Contains(pane))
            throw new InvalidOperationException($"{pane} is already registered");

        _panes.Add(pane);
        pane.OnBoundsChange += UpdateBounds;

        return () =>
        {
            if (!_panes.Remove(pane))
                throw new InvalidOperationException($"{pane} is not registered");

            pane.OnBoundsChange -= UpdateBounds;
        };
    }

    /// <summary>
    /// Updates the chart view based on the current moment and size.
    /// </summary>
    public void Update()
    {
        var size = Rect.Width.FloorInt32() * Duration.FromMilliseconds(MsPerPx);
        _view.Set(Moment - size, Moment);
        Updated();
    }

    /// <summary>
    /// Sets the lookup position and raises the lookup changed event.
    /// </summary>
    /// <param name="moment">The moment in time for the lookup.</param>
    /// <param name="point">The point coordinates for the lookup.</param>
    public void SetLookup(Instant? moment, Point? point) => LookupChanged(moment, point);

    /// <summary>
    /// Attempts to mark the canvas as clean for drawing.
    /// </summary>
    /// <returns>True if the canvas was dirty and is now marked clean; otherwise, false.</returns>
    public bool TryDraw() => Interlocked.CompareExchange(ref _isCanvasDirty, 0, 1) == 1;

    /// <summary>
    /// Attempts to get the current overlay request.
    /// </summary>
    /// <param name="point">When this method returns, contains the overlay point if available.</param>
    /// <returns>True if an overlay was requested; otherwise, false.</returns>
    public bool TryOverlay(out Point point)
    {
        var (p, isRequested) = _overlayRequest;
        _overlayRequest = default;
        point = p ?? default;

        return isRequested;
    }

    /// <summary>
    /// Requests that the canvas be redrawn.
    /// </summary>
    public void RequestDraw() => Volatile.Write(ref _isCanvasDirty, 1);

    /// <summary>
    /// Requests an overlay at the specified point.
    /// </summary>
    /// <param name="point">The point for the overlay request.</param>
    public void RequestOverlay(Point? point) => _overlayRequest = (point, true);

    /// <summary>
    /// Sets the current moment in time for the chart.
    /// </summary>
    /// <param name="moment">The moment to set.</param>
    public void SetMoment(Instant moment)
    {
        Moment = moment;
    }

    /// <summary>
    /// Sets the zoom level for the chart.
    /// </summary>
    /// <param name="zoom">The zoom level to set.</param>
    public void SetZoom(int zoom)
    {
        if (!_zooms.Contains(zoom))
            throw new ArgumentOutOfRangeException($"Zoom value {zoom} is not valid");

        Zoom = zoom;
        UpdateUnits();
        RequestDraw();
    }

    /// <summary>
    /// Sets the time resolution for the chart.
    /// </summary>
    /// <param name="resolution">The time resolution to set.</param>
    public void SetResolution(Duration resolution)
    {
        if (!_resolutions.Contains(resolution))
            throw new ArgumentOutOfRangeException($"Resolution value {resolution} is not valid");

        Resolution = resolution;
        UpdateUnits();
        foreach (var series in _panes.SelectMany(pane => pane.Sources))
            series.SetResolution(Resolution);
        RequestDraw();
    }

    /// <summary>
    /// Updates the chart bounds based on pane bounds changes.
    /// </summary>
    /// <param name="bounds">The new bounds to incorporate.</param>
    private void UpdateBounds(ValueRange<Instant> bounds)
    {
        var (start, end) =
            _panes.Count == 0
                ? (FutureBound, PastBound)
                : (Instant.Min(_bounds.Start, bounds.Start), Instant.Max(_bounds.End, bounds.End));

        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.Set(start, end);
    }

    /// <summary>
    /// Updates the unit calculations based on current zoom and resolution.
    /// </summary>
    private void UpdateUnits()
    {
        MsPerPx = (NodaConstants.MillisecondsPerMinute * Resolution.TotalMinutes / Zoom).FloorInt32();
        PxPerResolution = Zoom;
    }

    /// <summary>
    /// Returns a string representation of the chart context.
    /// </summary>
    /// <returns>The full identifier of the chart context.</returns>
    public override string ToString() => this.GetFullId();
}
