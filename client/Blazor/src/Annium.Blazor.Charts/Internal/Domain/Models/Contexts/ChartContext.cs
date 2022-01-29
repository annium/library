using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.NodaTime.Extensions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record ChartContext : IManagedChartContext
{
    public event Action Updated = delegate { };
    public event Action<Instant?, Point?> LookupChanged = delegate { };
    public Instant Moment { get; private set; }
    public Element Container { get; private set; } = default!;
    public DomRect Rect { get; private set; }
    public IReadOnlyCollection<IPaneContext> Panes => _panes;
    public int MsPerPx { get; private set; }
    public DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    public ValueRange<Instant> Range => _range;
    public ValueRange<Instant> View => _view;

    public int Zoom => _zoom;
    public bool IsLocked => _panes.Any(x => x.IsLocked) || _sources.Any(x => x.IsLoading);

    private readonly HashSet<IPaneContext> _panes = new();
    private readonly HashSet<ISeriesSource> _sources = new();
    private readonly ValueRange<Instant> _bounds;
    private readonly ManagedValueRange<Instant> _view = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private readonly ManagedValueRange<Instant> _range = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private int _zoom;
    private decimal _rawZoom = ZoomDefault;
    private int _isCanvasDirty = 1;
    private (Point?, bool) _overlayRequest;

    public ChartContext(
        ITimeProvider timeProvider
    )
    {
        Moment = timeProvider.Now;
        _bounds = ValueRange.Create(
            () => _panes.Count > 0 ? _panes.Min(x => x.Bounds.Start) : timeProvider.Now,
            () => _panes.Count > 0 ? _panes.Max(x => x.Bounds.End) : timeProvider.Now
        );
        SetZoom(ZoomDefault);
    }

    public void Init(Element container)
    {
        Container = container;
        Rect = container.GetBoundingClientRect();
    }

    public void RegisterPane(IPaneContext paneContext)
    {
        if (!_panes.Add(paneContext))
            throw new InvalidOperationException("Pane is already registered");
    }

    public void RegisterSource(ISeriesSource source)
    {
        if (!_sources.Add(source))
            throw new InvalidOperationException("Source is already registered");
    }

    public bool HandleZoomEvent(decimal delta)
    {
        _rawZoom = (_rawZoom * (1 - delta * ZoomMultiplier)).Within(ZoomMin, ZoomMax);

        var value = Zooms.OrderBy(x => _rawZoom.DiffFrom(x)).First();
        if (value == _zoom)
            return false;

        SetZoom(value);

        return true;
    }

    public bool ChangeScroll(decimal delta)
    {
        var change = (delta * ScrollMultiplier).FloorInt32();
        if (change == 0)
            return false;

        var (start, end) = GetView();
        var size = end - start;
        if (change < 0 && end - _bounds.Start <= size / 2)
            return false;

        if (change > 0 && _bounds.End - start <= size / 2)
            return false;

        var duration = Duration.FromMilliseconds(MsPerPx * Math.Abs(change));
        if (change > 0)
            Moment += duration;
        else
            Moment -= duration;

        return true;
    }

    public void Adjust(Instant moment)
    {
        Moment = moment;
        var (start, end) = GetView();

        // Console.WriteLine($"range: {S(start)} - {S(end)} size: {size} bounds: {S(_bounds.Start)} - {S(_bounds.End)}");
        _range.SetStart(start.CeilToMinute());
        _range.SetEnd(end.FloorToMinute());
        _view.SetStart(start);
        _view.SetEnd(end);
    }

    public void SendUpdate() => Updated();
    public void SendLookupChanged(Instant? moment, Point? point) => LookupChanged(moment, point);

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

    private (Instant start, Instant end) GetView()
    {
        var msPerPx = MsPerPx;
        var size = Duration.FromMilliseconds(Rect.Width.FloorInt32() * msPerPx);
        var start = Moment - size;

        return (start, Moment);
    }

    public void SetZoom(int zoom)
    {
        _zoom = zoom;
        MsPerPx = ((double)NodaConstants.MillisecondsPerMinute / zoom).FloorInt32();
    }
}