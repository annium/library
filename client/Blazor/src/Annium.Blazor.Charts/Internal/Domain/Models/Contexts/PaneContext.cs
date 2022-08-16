using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record PaneContext : IManagedPaneContext, ILogSubject
{
    public IReadOnlyCollection<ISeriesSource> Sources => _sources;
    public ISeriesContext Series { get; private set; } = default!;
    public IHorizontalSideContext? Bottom { get; set; }
    public IVerticalSideContext? Right { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public decimal DotPerPx => (View.End - View.Start) / Height;
    public bool IsLocked => _sources.Any(x => x.IsLoading);
    public ValueRange<Instant> Bounds { get; }
    public ValueRange<decimal> Range { get; }
    public ValueRange<decimal> View { get; }
    public ILogger Logger { get; }
    private readonly HashSet<ISeriesSource> _sources = new();
    private readonly Dictionary<ISeriesSource, ManagedValueRange<decimal>> _sourceRanges = new();
    private IChartContext _chartContext = default!;
    private int _isInitiated;

    public PaneContext(
        ITimeProvider timeProvider,
        ILogger<PaneContext> logger
    )
    {
        Bounds = ValueRange.Create(
            () => _sources.Count > 0 ? _sources.Min(x => x.Bounds.Start) : timeProvider.Now,
            () => _sources.Count > 0 ? _sources.Max(x => x.Bounds.End) : timeProvider.Now
        );
        Range = ValueRange.Create(
            () => _sourceRanges.Count > 0 ? _sourceRanges.Min(x => x.Value.Start) : 0m,
            () => _sourceRanges.Count > 0 ? _sourceRanges.Max(x => x.Value.End) : 0m
        );
        View = ValueRange.Create(GetViewStart, GetViewEnd);
        Logger = logger;
    }

    public void Init(
        IChartContext chartContext
    )
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(VerticalSideContext)} more than once");

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
        this.Log().Trace($"adjust range of {source.GetType().FriendlyName()} to {min} - {max}");
        var sourceRange = _sourceRanges[source];
        sourceRange.SetStart(min);
        sourceRange.SetEnd(max);

        var changed = Range.Start != allMin || Range.End != allMax;
        if (changed)
            _chartContext.RequestDraw();

        return changed;
    }

    public Action RegisterSource(ISeriesSource source)
    {
        if (!_sources.Add(source))
            throw new InvalidOperationException("Source is already registered");
        _sourceRanges[source] = ValueRange.Create(0m, 0m);

        return () =>
        {
            if (!_sources.Remove(source))
                throw new InvalidOperationException("Source is not registered");
            _sourceRanges.Remove(source);
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
}