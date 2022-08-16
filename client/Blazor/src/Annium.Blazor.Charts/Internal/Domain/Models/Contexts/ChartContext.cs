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
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record ChartContext : IManagedChartContext
{
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
    public ValueRange<Instant> Bounds { get; }
    public ValueRange<Instant> View => _view;
    public ValueRange<Instant> Range => _range;
    public IReadOnlyCollection<IPaneContext> Panes => _panes;

    private List<int> _zooms = new() { 1 };
    private List<Duration> _resolutions = new() { Duration.FromMinutes(1) };
    private readonly HashSet<IPaneContext> _panes = new();
    private readonly ManagedValueRange<Instant> _view = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private readonly ManagedValueRange<Instant> _range = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private DomRect _rect;
    private int _isCanvasDirty = 1;
    private (Point?, bool) _overlayRequest;

    public ChartContext(
        ITimeProvider timeProvider
    )
    {
        Moment = timeProvider.Now;
        Bounds = ValueRange.Create(
            () => _panes.Count > 0 ? _panes.Min(x => x.Bounds.Start) : timeProvider.Now,
            () => _panes.Count > 0 ? _panes.Max(x => x.Bounds.End) : timeProvider.Now
        );
        SetZoom(_zooms[0]);
        SetResolution(_resolutions[0]);
    }

    public void Init(Element container)
    {
        _rect = container.GetBoundingClientRect();
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

    public Action RegisterPane(IPaneContext paneContext)
    {
        if (!_panes.Add(paneContext))
            throw new InvalidOperationException("Pane is already registered");

        return () =>
        {
            if (!_panes.Remove(paneContext))
                throw new InvalidOperationException("Pane is not registered");
        };
    }

    public void Update()
    {
        var (start, end) = GetView();

        _range.SetStart(start.FloorTo(Resolution));
        _range.SetEnd(end.CeilTo(Resolution));
        _view.SetStart(start);
        _view.SetEnd(end);

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
            series.Clear();
        RequestDraw();
    }

    private (Instant start, Instant end) GetView()
    {
        var size = _rect.Width.FloorInt32() * Duration.FromMilliseconds(MsPerPx);
        var start = Moment - size;

        return (start, Moment);
    }

    private void UpdateUnits()
    {
        MsPerPx = (NodaConstants.MillisecondsPerMinute * Resolution.TotalMinutes / Zoom).FloorInt32();
        PxPerResolution = Zoom;
    }
}