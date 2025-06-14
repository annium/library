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

/// <summary>
/// Provides context for a chart pane that contains series data and manages data sources.
/// </summary>
/// <param name="Logger">The logger instance for this pane context.</param>
internal sealed record PaneContext(ILogger Logger) : IManagedPaneContext, ILogSubject
{
    /// <summary>
    /// Event raised when the bounds of the pane change.
    /// </summary>
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };

    /// <summary>
    /// Gets the parent chart context.
    /// </summary>
    public IChartContext Chart { get; private set; } = null!;

    /// <summary>
    /// Gets the collection of data sources registered with this pane.
    /// </summary>
    public IReadOnlyCollection<ISeriesSource> Sources => _sources;

    /// <summary>
    /// Gets the series context for rendering series data.
    /// </summary>
    public ISeriesContext? Series { get; private set; }

    /// <summary>
    /// Gets or sets the bottom horizontal side context.
    /// </summary>
    public IHorizontalSideContext? Bottom { get; set; }

    /// <summary>
    /// Gets or sets the right vertical side context.
    /// </summary>
    public IVerticalSideContext? Right { get; set; }

    /// <summary>
    /// Gets the DOM rectangle representing the pane's bounds.
    /// </summary>
    public DomRect Rect { get; private set; }

    /// <summary>
    /// Gets the number of data units per pixel for vertical scaling.
    /// </summary>
    public decimal DotPerPx { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the pane is locked (any source is loading).
    /// </summary>
    public bool IsLocked => _sources.Any(x => x.IsLoading);

    /// <summary>
    /// Gets the time bounds of all data in the pane.
    /// </summary>
    public ValueRange<Instant> Bounds => _bounds;

    /// <summary>
    /// Gets the value range of all data in the pane.
    /// </summary>
    public ValueRange<decimal> Range => _range;

    /// <summary>
    /// Gets the currently visible value range with padding.
    /// </summary>
    public ValueRange<decimal> View => _view;

    /// <summary>
    /// The managed time bounds of all data in the pane.
    /// </summary>
    private readonly ManagedValueRange<Instant> _bounds = ValueRange.Create(FutureBound, PastBound);

    /// <summary>
    /// The managed value range of all data in the pane.
    /// </summary>
    private readonly ManagedValueRange<decimal> _range = ValueRange.Create(decimal.MinValue, decimal.MaxValue);

    /// <summary>
    /// The managed currently visible value range with padding.
    /// </summary>
    private readonly ManagedValueRange<decimal> _view = ValueRange.Create(decimal.MinValue, decimal.MaxValue);

    /// <summary>
    /// The collection of registered data sources.
    /// </summary>
    private readonly List<ISeriesSource> _sources = new();

    /// <summary>
    /// The dictionary mapping sources to their individual value ranges.
    /// </summary>
    private readonly Dictionary<ISeriesSource, ManagedValueRange<decimal>> _sourceRanges = new();

    /// <summary>
    /// Flag indicating whether the context has been initiated.
    /// </summary>
    private int _isInitiated;

    /// <summary>
    /// Initializes the pane context with a parent chart context.
    /// </summary>
    /// <param name="chart">The parent chart context.</param>
    public void Init(IChartContext chart)
    {
        if (Interlocked.CompareExchange(ref _isInitiated, 1, 0) != 0)
            throw new InvalidOperationException($"Can't init {nameof(PaneContext)} more than once");

        Chart = chart;
    }

    /// <summary>
    /// Sets the DOM rectangle bounds for the pane.
    /// </summary>
    /// <param name="rect">The DOM rectangle to set.</param>
    public void SetRect(DomRect rect)
    {
        Rect = rect;
        Chart.RequestDraw();
        UpdateDotPerPx();
    }

    /// <summary>
    /// Adjusts the value range for a specific data source and recalculates the overall pane range.
    /// </summary>
    /// <param name="source">The data source whose range is being adjusted.</param>
    /// <param name="min">The minimum value for the source.</param>
    /// <param name="max">The maximum value for the source.</param>
    /// <returns>True if the pane range was changed; otherwise, false.</returns>
    public bool AdjustRange(ISeriesSource source, decimal min, decimal max)
    {
        if (min > max)
            throw new ArgumentException($"Invalid range: {min} - {max}");

        var (start, end) = Range;
        var sourceRange = _sourceRanges[source];
        sourceRange.Set(min, max);

        var renderingRanges = _sourceRanges
            .Values.Where(x => x.Start != decimal.MinValue || x.End != decimal.MaxValue)
            .ToArray();
        if (renderingRanges.Length > 0)
        {
            this.Trace("update Pane range from {renderingRangesLength} rendering source(s)", renderingRanges.Length);
            _range.Set(renderingRanges.Min(x => x.Start), renderingRanges.Max(x => x.End));
        }
        else
        {
            this.Trace("reset Pane range due to lack of rendering source(s)");
            _range.Set(decimal.MinValue, decimal.MaxValue);
        }

        if (Range.Start == start && Range.End == end)
        {
            this.Trace(
                "range of {sourceId} updated to {min} - {max}, not changed Pane range: {Range}",
                source.GetFullId(),
                min,
                max,
                Range
            );
            return false;
        }

        this.Trace(
            "range of {sourceId} updated to {min} - {max}, adjusted Pane range: {start} - {end} -> {Range}",
            min,
            max,
            start,
            end,
            Range
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

    /// <summary>
    /// Registers a data source with the pane.
    /// </summary>
    /// <param name="source">The data source to register.</param>
    /// <returns>A disposable that can be used to unregister the source.</returns>
    public IDisposable RegisterSource(ISeriesSource source)
    {
        if (_sources.Contains(source))
        {
            this.Trace<string>("source {sourceId} is already tracked", source.GetFullId());
            return Disposable.Empty;
        }

        this.Trace<string>("track source {sourceId}", source.GetFullId());
        _sources.Add(source);
        _sourceRanges[source] = ValueRange.Create(decimal.MinValue, decimal.MaxValue);
        source.OnBoundsChange += UpdateBounds;
        Chart.RequestDraw();

        return Disposable.Create(() =>
        {
            if (!_sources.Remove(source))
                throw new InvalidOperationException("Source is not registered");

            this.Trace<string>("untrack source {sourceId}", source.GetFullId());
            _sourceRanges.Remove(source);
            source.OnBoundsChange -= UpdateBounds;
            Chart.RequestDraw();

            if (_sources.Count == 0)
                ResetRangeAndView();
        });
    }

    /// <summary>
    /// Sets the series context for rendering series data.
    /// </summary>
    /// <param name="series">The series context to set.</param>
    public void SetSeries(ISeriesContext? series)
    {
        Series = series;
        Chart.RequestDraw();
    }

    /// <summary>
    /// Sets the bottom horizontal side context.
    /// </summary>
    /// <param name="bottom">The bottom horizontal side context to set.</param>
    public void SetBottom(IHorizontalSideContext? bottom)
    {
        Bottom = bottom;
        Chart.RequestDraw();
    }

    /// <summary>
    /// Sets the right vertical side context.
    /// </summary>
    /// <param name="right">The right vertical side context to set.</param>
    public void SetRight(IVerticalSideContext? right)
    {
        Right = right;
        Chart.RequestDraw();
    }

    /// <summary>
    /// Updates the dots per pixel calculation based on current view and rectangle height.
    /// </summary>
    private void UpdateDotPerPx()
    {
        if (Rect.Height == 0 || _view is { Start: decimal.MinValue, End: decimal.MaxValue })
            DotPerPx = 0;
        else
            DotPerPx = (_view.End - _view.Start) / Rect.Height;
    }

    /// <summary>
    /// Updates the pane bounds based on source bounds changes.
    /// </summary>
    /// <param name="bounds">The new bounds to incorporate.</param>
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

    /// <summary>
    /// Resets the range and view when no sources are registered.
    /// </summary>
    private void ResetRangeAndView()
    {
        this.Trace(string.Empty);
        _range.Set(0m, 0m);
        _view.Set(0m, 0m);
        UpdateDotPerPx();
    }

    /// <summary>
    /// Returns a string representation of the pane context.
    /// </summary>
    /// <returns>The full identifier of the pane context.</returns>
    public override string ToString() => this.GetFullId();
}
