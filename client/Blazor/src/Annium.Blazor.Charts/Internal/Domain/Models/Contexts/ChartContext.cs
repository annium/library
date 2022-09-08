using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record ChartContext : IManagedChartContext, ILogSubject<ChartContext>
{
    public ILogger<ChartContext> Logger { get; }
    public event Action Updated = delegate { };
    public event Action<Instant?, Point?> LookupChanged = delegate { };
    public Instant Moment { get; private set; }
    public int Zoom { get; private set; }
    public IReadOnlyList<int> Zooms => _zooms;
    public Duration Resolution { get; private set; }
    public int PxPerResolution { get; private set; }
    public IReadOnlyList<Duration> Resolutions => _resolutions;
    public bool IsLocked => _panes.Any(x => x.IsLocked);
    public int MsPerPx { get; private set; }
    public DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    public int TimeZoneOffset { get; } = DateTimeZoneProviders.Tzdb.GetSystemDefault().GetUtcOffset(NodaConstants.UnixEpoch).ToTimeSpan().TotalMinutes.FloorInt32();
    public ValueRange<Instant> Bounds => _bounds;
    public ValueRange<Instant> View => _view;
    public IReadOnlyCollection<IPaneContext> Panes => _panes;
    public DomRect Rect { get; private set; }

    private List<int> _zooms = new() { 1 };
    private List<Duration> _resolutions = new() { Duration.FromMinutes(1) };
    private readonly List<IPaneContext> _panes = new();
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);
    private readonly ManagedValueRange<Instant> _view = ValueRange.Create(FutureBound, PastBound);
    private int _isCanvasDirty = 1;
    private (Point?, bool) _overlayRequest;

    public ChartContext(
        ITimeProvider timeProvider,
        ILogger<ChartContext> logger
    )
    {
        Logger = logger;
        Moment = timeProvider.Now;
        SetZoom(_zooms[0]);
        SetResolution(_resolutions[0]);
    }

    public void SetRect(DomRect rect)
    {
        Rect = rect;
        RequestDraw();
    }

    public void Configure(
        IReadOnlyList<int> zooms,
        IReadOnlyList<int> resolutions
    )
    {
        if (zooms.Count == 0)
            throw new ArgumentException("Zooms list is empty");

        if (resolutions.Count == 0)
            throw new ArgumentException("Resolutions list is empty");

        _zooms = zooms.ToList();
        SetZoom(_zooms[(_zooms.Count / (decimal) 2).FloorInt32()]);
        _resolutions = resolutions.Select(i => Duration.FromMinutes(i)).ToList();
        SetResolution(_resolutions[0]);

        RequestDraw();
    }

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

    public void Update()
    {
        var size = Rect.Width.FloorInt32() * Duration.FromMilliseconds(MsPerPx);
        _view.Set(Moment - size, Moment);
        Updated();
    }

    public void SetLookup(Instant? moment, Point? point) => LookupChanged(moment, point);

    public bool TryDraw() => Interlocked.CompareExchange(ref _isCanvasDirty, 0, 1) == 1;

    public bool TryOverlay(out Point point)
    {
        var (p, isRequested) = _overlayRequest;
        _overlayRequest = default;
        point = p ?? default;

        return isRequested;
    }

    public void RequestDraw() => Volatile.Write(ref _isCanvasDirty, 1);

    public void RequestOverlay(Point? point) => _overlayRequest = (point, true);

    public void SetMoment(Instant moment)
    {
        Moment = moment;
    }

    public void SetZoom(int zoom)
    {
        if (!_zooms.Contains(zoom))
            throw new ArgumentOutOfRangeException($"Zoom value {zoom} is not valid");

        Zoom = zoom;
        UpdateUnits();
        RequestDraw();
    }

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

    private void UpdateBounds(ValueRange<Instant> bounds)
    {
        var (start, end) = _panes.Count == 0
            ? (FutureBound, PastBound)
            : (Instant.Min(_bounds.Start, bounds.Start), Instant.Max(_bounds.End, bounds.End));

        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.Set(start, end);
    }

    private void UpdateUnits()
    {
        MsPerPx = (NodaConstants.MillisecondsPerMinute * Resolution.TotalMinutes / Zoom).FloorInt32();
        PxPerResolution = Zoom;
    }

    public override string ToString() => this.GetFullId();
}