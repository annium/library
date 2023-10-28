using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Data.Models;
using Annium.Logging;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record PaneContext(ILogger Logger) : IManagedPaneContext, ILogSubject
{
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public IChartContext Chart { get; private set; } = default!;
    public IReadOnlyCollection<ISeriesSource> Sources => _sources;
    public ISeriesContext? Series { get; private set; }
    public IHorizontalSideContext? Bottom { get; set; }
    public IVerticalSideContext? Right { get; set; }
    public DomRect Rect { get; private set; }
    public decimal DotPerPx { get; private set; }
    public bool IsLocked => _sources.Any(x => x.IsLoading);
    public ValueRange<Instant> Bounds => _bounds;
    public ValueRange<decimal> Range => _range;
    public ValueRange<decimal> View => _view;
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);
    private readonly ManagedValueRange<decimal> _range = ValueRange.Create(decimal.MinValue, decimal.MaxValue);
    private readonly ManagedValueRange<decimal> _view = ValueRange.Create(decimal.MinValue, decimal.MaxValue);
    private readonly List<ISeriesSource> _sources = new();
    private readonly Dictionary<ISeriesSource, ManagedValueRange<decimal>> _sourceRanges = new();
    private int _isInitiated;

    public void Init(IChartContext chart)
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(PaneContext)} more than once");

        Chart = chart;
    }

    public void SetRect(DomRect rect)
    {
        Rect = rect;
        Chart.RequestDraw();
        UpdateDotPerPx();
    }

    public bool AdjustRange(ISeriesSource source, decimal min, decimal max)
    {
        if (min > max)
            throw new ArgumentException($"Invalid range: {min} - {max}");

        var (start, end) = Range;
        var sourceRange = _sourceRanges[source];
        sourceRange.Set(min, max);

        var renderingRanges = _sourceRanges.Values
            .Where(x => x.Start != decimal.MinValue || x.End != decimal.MaxValue)
            .ToArray();
        if (renderingRanges.Length > 0)
        {
            this.Trace($"update Pane range from {renderingRanges.Length} rendering source(s)");
            _range.Set(renderingRanges.Min(x => x.Start), renderingRanges.Max(x => x.End));
        }
        else
        {
            this.Trace("reset Pane range due to lack of rendering source(s)");
            _range.Set(decimal.MinValue, decimal.MaxValue);
        }

        if (Range.Start == start && Range.End == end)
        {
            this.Trace($"range of {source.GetFullId()} updated to {min} - {max}, not changed Pane range: {Range}");
            return false;
        }

        this.Trace(
            $"range of {source.GetFullId()} updated to {min} - {max}, adjusted Pane range: {start} - {end} -> {Range}"
        );
        (start, end) = Range;

        // no rendering ranges
        if (start == decimal.MinValue && end == decimal.MaxValue)
            _view.Set(decimal.MinValue, decimal.MaxValue);
        else
        {
            var size = Math.Abs(end - start);
            if (size > 0)
                _view.Set(start - size * 0.1m, end + size * 0.1m);
            else if (start == 0)
                _view.Set(-0.5m, 0.5m);
            else
                _view.Set(start * 0.9m, start * 1.1m);
        }

        UpdateDotPerPx();

        Chart.RequestDraw();

        return true;
    }

    public IDisposable RegisterSource(ISeriesSource source)
    {
        if (_sources.Contains(source))
        {
            this.Trace($"source {source.GetFullId()} is already tracked");
            return Disposable.Empty;
        }

        this.Trace($"track source {source.GetFullId()}");
        _sources.Add(source);
        _sourceRanges[source] = ValueRange.Create(decimal.MinValue, decimal.MaxValue);
        source.OnBoundsChange += UpdateBounds;
        Chart.RequestDraw();

        return Disposable.Create(() =>
        {
            if (!_sources.Remove(source))
                throw new InvalidOperationException("Source is not registered");

            this.Trace($"untrack source {source.GetFullId()}");
            _sourceRanges.Remove(source);
            source.OnBoundsChange -= UpdateBounds;
            Chart.RequestDraw();

            if (_sources.Count == 0)
                ResetRangeAndView();
        });
    }

    public void SetSeries(ISeriesContext? series)
    {
        Series = series;
        Chart.RequestDraw();
    }

    public void SetBottom(IHorizontalSideContext? bottom)
    {
        Bottom = bottom;
        Chart.RequestDraw();
    }

    public void SetRight(IVerticalSideContext? right)
    {
        Right = right;
        Chart.RequestDraw();
    }

    private void UpdateDotPerPx()
    {
        if (Rect.Height == 0 || _view is { Start: decimal.MinValue, End: decimal.MaxValue })
            DotPerPx = 0;
        else
            DotPerPx = (_view.End - _view.Start) / Rect.Height;
    }

    private void UpdateBounds(ValueRange<Instant> bounds)
    {
        var (start, end) =
            _sources.Count == 0
                ? (FutureBound, PastBound)
                : (Instant.Min(_bounds.Start, bounds.Start), Instant.Max(_bounds.End, bounds.End));

        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.Set(start, end);
        OnBoundsChange(_bounds);
    }

    private void ResetRangeAndView()
    {
        this.Trace(string.Empty);
        _range.Set(0m, 0m);
        _view.Set(0m, 0m);
        UpdateDotPerPx();
    }

    public override string ToString() => this.GetFullId();
}
