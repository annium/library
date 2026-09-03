using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Blazor component for rendering node series charts with custom rendering delegates for each point.
/// </summary>
/// <typeparam name="T">The type implementing IPointValue interface representing point data.</typeparam>
public partial class NodeSeries<T> : SeriesBase<T>, ILogSubject
    where T : IPointValue
{
    /// <summary>
    /// Delegate for custom rendering of individual node items.
    /// </summary>
    /// <param name="item">The point item to render.</param>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="x">The x-coordinate for rendering.</param>
    /// <param name="y">The y-coordinate for rendering.</param>
    public delegate void Renderer(T item, Canvas ctx, int x, int y);

    /// <summary>
    /// Gets or sets the custom renderer delegate for rendering individual node items.
    /// </summary>
    [Parameter, EditorRequired]
    public Renderer RenderItem { get; set; } = delegate { };

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets the minimum number of values required to render the series.
    /// </summary>
    protected override int MinValuesToRender => 1;

    /// <summary>
    /// Renders the node series values using the custom renderer delegate.
    /// </summary>
    /// <param name="items">The collection of point values to render.</param>
    protected override void RenderValues(IReadOnlyList<T> items)
    {
        foreach (var item in items)
            RenderItem(item, SeriesContext.Canvas, PaneContext.ToX(item.Moment), PaneContext.ToY(item.Value));
    }

    /// <summary>
    /// Calculates the minimum and maximum values from the point data for chart scaling.
    /// </summary>
    /// <param name="items">The collection of point items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Value);
            max = Math.Max(max, item.Value);
        }

        return (min, max);
    }
}
