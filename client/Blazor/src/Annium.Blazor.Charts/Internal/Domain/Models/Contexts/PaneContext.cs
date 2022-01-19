using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Core.Primitives;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Domain.Models.Contexts;

internal sealed record PaneContext : IManagedPaneContext
{
    private const int DefaultBlockSize = 40;

    public IReadOnlyCollection<ISeriesSource> Sources => _sources;
    public ISeriesContext Series { get; private set; } = default!;
    public IHorizontalSideContext? Bottom { get; set; }
    public IVerticalSideContext? Right { get; set; }
    public decimal DotPerPx => (View.End - View.Start) / _height;
    public bool IsLocked => _sources.Any(x => x.IsLoading);
    public ValueRange<Instant> Bounds { get; }
    public ValueRange<decimal> Range => _range;
    public ValueRange<decimal> View { get; }
    public IReadOnlyDictionary<int, decimal> HorizontalLines { get; private set; } = new Dictionary<int, decimal>();
    private readonly HashSet<ISeriesSource> _sources = new();
    private readonly ManagedValueRange<decimal> _range = ValueRange.Create(0m, 0m);
    private IChartContext _chartContext = default!;
    private int _height;

    public PaneContext(
        ITimeProvider timeProvider
    )
    {
        Bounds = ValueRange.Create(
            () => _sources.Count > 0 ? _sources.Min(x => x.Bounds.Start) : timeProvider.Now,
            () => _sources.Count > 0 ? _sources.Max(x => x.Bounds.End) : timeProvider.Now
        );

        View = ValueRange.Create(
            () => _range.Start - (_range.End - _range.Start) * 0.1m,
            () => _range.End + (_range.End - _range.Start) * 0.1m
        );
    }

    public void Init(
        IChartContext chartContext
    )
    {
        _chartContext = chartContext;
    }

    public void SetHeight(int height) => _height = height;

    public bool AdjustRange(decimal min, decimal max)
    {
        if (min >= max)
            throw new ArgumentException($"Invalid range: {min} - {max}");

        if (_range.Start <= min && max <= _range.End)
            return false;

        _range.SetStart(min);
        _range.SetEnd(max);

        HorizontalLines = GetHorizontalLines();

        _chartContext.RequestDraw();

        return true;
    }

    public void RegisterSource(ISeriesSource source)
    {
        if (!_sources.Add(source))
            throw new InvalidOperationException("Source is already registered");
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

    private IReadOnlyDictionary<int, decimal> GetHorizontalLines()
    {
        var (min, max) = View;
        var dpx = DotPerPx;
        var alignment = (DefaultBlockSize * dpx).ToPretty(0.5m);

        var lines = new Dictionary<int, decimal>();
        var value = min.CeilTo(alignment);

        while (value <= max)
        {
            var line = _height - ((value - min) / dpx).FloorInt32();
            lines[line] = value;
            value += alignment;
        }

        return lines;
    }
}