using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Data;
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
    private const int DefaultBlockSize = 80;

    public event Action Updated = delegate { };
    public Element Container { get; private set; } = default!;
    public DomRect Rect { get; private set; }
    public IReadOnlyCollection<IPaneContext> Panes => _panes;
    public int MsPerPx => ((double)NodaConstants.MillisecondsPerMinute / Zoom).FloorInt32();
    public DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    public ValueRange<Instant> Range => _range;
    public ValueRange<Instant> View => _view;
    public Instant? LookupMoment { get; private set; }
    public IReadOnlyDictionary<int, LocalDateTime> VerticalLines { get; private set; } = new Dictionary<int, LocalDateTime>();

    public int Scroll => _scroll;
    public int Zoom => _zoom;
    public bool IsLocked => _panes.Any(x => x.IsLocked) || _sources.Any(x => x.IsLoading);

    private readonly HashSet<IPaneContext> _panes = new();
    private readonly HashSet<ISeriesSource> _sources = new();
    private readonly ValueRange<Instant> _bounds;
    private readonly ManagedValueRange<Instant> _view = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private readonly ManagedValueRange<Instant> _range = ValueRange.Create(Instant.MinValue, Instant.MinValue);
    private Instant _moment;
    private int _scroll;
    private int _zoom = ZoomDefault;
    private decimal _rawZoom = ZoomDefault;
    private int _isDirty = 1;

    public ChartContext(
        ITimeProvider timeProvider
    )
    {
        _bounds = ValueRange.Create(
            () => _panes.Count > 0 ? _panes.Min(x => x.Bounds.Start) : timeProvider.Now,
            () => _panes.Count > 0 ? _panes.Max(x => x.Bounds.End) : timeProvider.Now
        );
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

    public void SetLookupMoment(Instant? moment) => LookupMoment = moment;

    public bool ChangeZoom(decimal delta)
    {
        _rawZoom = (_rawZoom * (1 - delta * ZoomMultiplier)).Within(ZoomMin, ZoomMax);

        var value = Zooms.OrderBy(x => _rawZoom.DiffFrom(x)).First();
        if (value == _zoom)
            return false;

        _zoom = value;

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

        var scroll = Scroll + change;
        if (Scroll == scroll)
            return false;

        _scroll = scroll;

        return true;
    }

    public void Adjust(Instant moment)
    {
        _moment = moment;
        var msPerPx = MsPerPx;
        var (start, end) = GetView();

        // Console.WriteLine($"range: {S(start)} - {S(end)} size: {size} bounds: {S(_bounds.Start)} - {S(_bounds.End)}");
        _range.SetStart(start.CeilToMinute());
        _range.SetEnd(end.FloorToMinute());
        _view.SetStart(start);
        _view.SetEnd(end);

        var alignment = GetAlignmentDuration(msPerPx);
        var verticalLines = new Dictionary<int, LocalDateTime>();
        var lineMoment = start.FloorTo(alignment);

        // align floors instant, so pick next period if start is not aligned
        if (lineMoment < start)
            lineMoment += alignment;

        while (lineMoment <= end)
        {
            var line = ((lineMoment - start).TotalMilliseconds.FloorInt64() / (decimal)msPerPx).FloorInt32();
            verticalLines[line] = lineMoment.InZone(TimeZone).LocalDateTime;
            lineMoment += alignment;
        }

        VerticalLines = verticalLines;
    }

    public void SendUpdate() => Updated();

    public bool TryDraw() => Interlocked.CompareExchange(ref _isDirty, 0, 1) == 1;
    public void RequestDraw() => Volatile.Write(ref _isDirty, 1);

    private (Instant start, Instant end) GetView()
    {
        var msPerPx = MsPerPx;
        var size = Duration.FromMilliseconds(Rect.Width.FloorInt32() * msPerPx);
        var offset = Duration.FromMilliseconds(Math.Abs(Scroll) * msPerPx);
        var end = Scroll > 0 ? _moment + offset : _moment - offset;
        var start = end - size;

        return (start, end);
    }

    private Duration GetAlignmentDuration(long msPerPx)
    {
        var block = (decimal)DefaultBlockSize * msPerPx / NodaConstants.MillisecondsPerMinute;

        return block switch
        {
            > 240 => Duration.FromHours(4),
            > 120 => Duration.FromHours(2),
            > 60  => Duration.FromHours(1),
            > 30  => Duration.FromMinutes(30),
            > 15  => Duration.FromMinutes(15),
            _     => Duration.FromMinutes(block > 5 ? 5 : 3)
        };
    }
}