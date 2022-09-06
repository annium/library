using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record PaneContext(ILogger<PaneContext> Logger) : IManagedPaneContext, ILogSubject<PaneContext>
{
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public IChartContext Chart { get; private set; } = default!;
    public IReadOnlyCollection<ISeriesSource> Sources => _sources;
    public ISeriesContext Series { get; private set; } = default!;
    public IHorizontalSideContext? Bottom { get; set; }
    public IVerticalSideContext? Right { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public decimal DotPerPx => (View.End - View.Start) / Height;
    public bool IsLocked => _sources.Any(x => x.IsLoading);
    public ValueRange<Instant> Bounds => _bounds;
    public ValueRange<decimal> Range => _range;
    public ValueRange<decimal> View => _view;
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);
    private readonly ManagedValueRange<decimal> _range = ValueRange.Create(0m, 0m);
    private readonly ManagedValueRange<decimal> _view = ValueRange.Create(0m, 0m);
    private readonly List<ISeriesSource> _sources = new();
    private readonly Dictionary<ISeriesSource, ManagedValueRange<decimal>> _sourceRanges = new();
    private int _isInitiated;

    public void Init(
        IChartContext chart
    )
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(PaneContext)} more than once");

        Chart = chart;
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool AdjustRange(ISeriesSource source, decimal min, decimal max)
    {
        if (min > max)
            throw new ArgumentException($"Invalid range: {min} - {max}");

        var (start, end) = Range;
        var sourceRange = _sourceRanges[source];
        sourceRange.Set(min, max);
        _range.Set(
            _sourceRanges.Min(x => x.Value.Start),
            _sourceRanges.Max(x => x.Value.End)
        );

        if (Range.Start == start && Range.End == end)
            return false;

        (start, end) = Range;
        var size = Math.Abs(end - start);
        if (size > 0)
            _view.Set(start - size * 0.1m, end + size * 0.1m);
        else if (start == 0)
            _view.Set(-0.5m, 0.5m);
        else
            _view.Set(start * 0.9m, start * 1.1m);

        this.Log().Trace($"range of {source.GetFullId()} updated to {min} - {max}, adjusted Pane range: {start} - {end} -> {Range}");
        Chart.RequestDraw();

        return true;
    }

    public Action RegisterSource(ISeriesSource source)
    {
        if (_sources.Contains(source))
            throw new InvalidOperationException("Source is already registered");

        _sources.Add(source);
        _sourceRanges[source] = ValueRange.Create(0m, 0m);
        source.OnBoundsChange += UpdateBounds;

        return () =>
        {
            if (!_sources.Remove(source))
                throw new InvalidOperationException("Source is not registered");

            _sourceRanges.Remove(source);
            source.OnBoundsChange -= UpdateBounds;
            if (_sources.Count == 0)
                ResetRangeAndView();
        };
    }

    public void SetSeries(ISeriesContext series)
    {
        if (Series is not null)
            throw new InvalidOperationException("Series is already set");

        Series = series;
    }

    public void SetBottom(IHorizontalSideContext bottom)
    {
        if (Bottom is not null)
            throw new InvalidOperationException("Bottom is already set");

        Bottom = bottom;
    }

    public void SetRight(IVerticalSideContext right)
    {
        if (Right is not null)
            throw new InvalidOperationException("Right is already set");

        Right = right;
    }

    private void UpdateBounds(ValueRange<Instant> bounds)
    {
        var (start, end) = _sources.Count == 0
            ? (FutureBound, PastBound)
            : (Instant.Min(_bounds.Start, bounds.Start), Instant.Max(_bounds.End, bounds.End));

        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.Set(start, end);
        OnBoundsChange(_bounds);
    }

    private void ResetRangeAndView()
    {
        _range.Set(0m, 0m);
        _view.Set(0m, 0m);
    }

    public override string ToString() => this.GetFullId();
}