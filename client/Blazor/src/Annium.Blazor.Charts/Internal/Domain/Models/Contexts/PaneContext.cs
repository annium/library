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

internal sealed record PaneContext : IManagedPaneContext, ILogSubject<PaneContext>
{
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
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
    public ValueRange<decimal> View { get; }
    public ILogger<PaneContext> Logger { get; }
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);
    private readonly ManagedValueRange<decimal> _range = ValueRange.Create(0m, 0m);
    private readonly List<ISeriesSource> _sources = new();
    private readonly Dictionary<ISeriesSource, ManagedValueRange<decimal>> _sourceRanges = new();
    private IChartContext _chartContext = default!;
    private int _isInitiated;

    public PaneContext(
        ILogger<PaneContext> logger
    )
    {
        View = ValueRange.Create(GetViewStart, GetViewEnd);
        Logger = logger;
    }

    public void Init(
        IChartContext chartContext
    )
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(PaneContext)} more than once");

        _chartContext = chartContext;
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

        var (allMin, allMax) = Range;
        var sourceRange = _sourceRanges[source];
        sourceRange.SetStart(min);
        sourceRange.SetEnd(max);
        _range.SetStart(_sourceRanges.Min(x => x.Value.Start));
        _range.SetEnd(_sourceRanges.Max(x => x.Value.End));

        if (Range.Start == allMin && Range.End == allMax)
            return false;

        this.Log().Trace($"range of {source.GetFullId()} updated to {min} - {max}, adjusted Pane range: {allMin} - {allMax} -> {Range}");
        _chartContext.RequestDraw();

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
                ResetRange();
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

        _bounds.SetStart(start);
        _bounds.SetEnd(end);
        OnBoundsChange(_bounds);
    }

    private void ResetRange()
    {
        _range.SetStart(0m);
        _range.SetEnd(0m);
    }

    private decimal GetViewStart()
    {
        var (start, end) = Range;
        var size = Math.Abs(end) - Math.Abs(start);
        if (size > 0)
            return Range.Start - size * 0.1m;

        return start == 0 ? 0.9m : start * 0.9m;
    }

    private decimal GetViewEnd()
    {
        var (start, end) = Range;
        var size = Math.Abs(end) - Math.Abs(start);
        if (size > 0)
            return Range.End + size * 0.1m;

        return end == 0 ? 1.1m : end * 1.1m;
    }

    public override string ToString() => this.GetFullId();
}