using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Interop;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Represents a pane context within a chart, managing series data and side components
/// </summary>
public interface IPaneContext
{
    /// <summary>
    /// Event triggered when the time bounds of the pane change
    /// </summary>
    event Action<ValueRange<Instant>> OnBoundsChange;

    /// <summary>
    /// Gets the parent chart context
    /// </summary>
    IChartContext Chart { get; }

    /// <summary>
    /// Gets the collection of series data sources
    /// </summary>
    IReadOnlyCollection<ISeriesSource> Sources { get; }

    /// <summary>
    /// Gets the series context for drawing series data
    /// </summary>
    ISeriesContext? Series { get; }

    /// <summary>
    /// Gets the bottom horizontal side context
    /// </summary>
    IHorizontalSideContext? Bottom { get; }

    /// <summary>
    /// Gets the right vertical side context
    /// </summary>
    IVerticalSideContext? Right { get; }

    /// <summary>
    /// Gets the DOM rectangle bounds of the pane
    /// </summary>
    DomRect Rect { get; }

    /// <summary>
    /// Gets the number of dots per pixel for value scaling
    /// </summary>
    decimal DotPerPx { get; }

    /// <summary>
    /// Gets a value indicating whether the pane is locked
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    /// Gets the time bounds of the pane data
    /// </summary>
    ValueRange<Instant> Bounds { get; }

    /// <summary>
    /// Gets the currently visible value range
    /// </summary>
    ValueRange<decimal> View { get; }

    /// <summary>
    /// Gets the full value range of the pane data
    /// </summary>
    ValueRange<decimal> Range { get; }

    /// <summary>
    /// Adjusts the value range for a specific series source
    /// </summary>
    /// <param name="source">The series source to adjust range for</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if the range was adjusted; otherwise, false</returns>
    bool AdjustRange(ISeriesSource source, decimal min, decimal max);

    /// <summary>
    /// Registers a series source with the pane
    /// </summary>
    /// <param name="source">The series source to register</param>
    /// <returns>A disposable to unregister the source</returns>
    IDisposable RegisterSource(ISeriesSource source);

    /// <summary>
    /// Sets the series context for the pane
    /// </summary>
    /// <param name="series">The series context to set, or null to clear</param>
    void SetSeries(ISeriesContext? series);

    /// <summary>
    /// Sets the bottom horizontal side context
    /// </summary>
    /// <param name="bottom">The bottom context to set, or null to clear</param>
    void SetBottom(IHorizontalSideContext? bottom);

    /// <summary>
    /// Sets the right vertical side context
    /// </summary>
    /// <param name="right">The right context to set, or null to clear</param>
    void SetRight(IVerticalSideContext? right);
}
