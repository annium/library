using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

public interface IPaneContext
{
    IReadOnlyCollection<ISeriesSource> Sources { get; }
    ISeriesContext Series { get; }
    IHorizontalSideContext? Bottom { get; }
    IVerticalSideContext? Right { get; }
    int Width { get; }
    int Height { get; }
    decimal DotPerPx { get; }
    bool IsLocked { get; }
    ValueRange<Instant> Bounds { get; }
    ValueRange<decimal> Range { get; }
    ValueRange<decimal> View { get; }
    bool AdjustRange(decimal min, decimal max);
    Action RegisterSource(ISeriesSource source);
    void SetSeries(ISeriesContext series);
    void SetBottom(IHorizontalSideContext bottom);
    void SetRight(IVerticalSideContext right);
}