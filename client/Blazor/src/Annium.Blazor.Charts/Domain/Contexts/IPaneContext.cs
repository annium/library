using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Interop;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IPaneContext
{
    event Action<ValueRange<Instant>> OnBoundsChange;
    IChartContext Chart { get; }
    IReadOnlyCollection<ISeriesSource> Sources { get; }
    ISeriesContext Series { get; }
    IHorizontalSideContext? Bottom { get; }
    IVerticalSideContext? Right { get; }
    DomRect Rect { get; }
    decimal DotPerPx { get; }
    bool IsLocked { get; }
    ValueRange<Instant> Bounds { get; }
    ValueRange<decimal> View { get; }
    ValueRange<decimal> Range { get; }
    bool AdjustRange(ISeriesSource source, decimal min, decimal max);
    Action RegisterSource(ISeriesSource source);
    void SetSeries(ISeriesContext series);
    void SetBottom(IHorizontalSideContext bottom);
    void SetRight(IVerticalSideContext right);
}