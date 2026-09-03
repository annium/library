using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Blazor component for rendering multiple node series charts with custom rendering delegates for each point item.
/// </summary>
/// <typeparam name="TM">The type implementing IMultiValue interface representing multi-value data points.</typeparam>
/// <typeparam name="TI">The type implementing IPointItem interface representing individual point items.</typeparam>
public partial class MultiNodeSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    /// <summary>
    /// Delegate for custom rendering of individual node items.
    /// </summary>
    /// <param name="item">The point item to render.</param>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="x">The x-coordinate for rendering.</param>
    /// <param name="y">The y-coordinate for rendering.</param>
    public delegate void Renderer(TI item, Canvas ctx, int x, int y);

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
    /// Renders the multi-node series values using the custom renderer delegate for each point item.
    /// </summary>
    /// <param name="values">The collection of multi-value items to render.</param>
    protected override void RenderValues(IReadOnlyList<TM> values)
    {
        foreach (var value in values)
        foreach (var item in value.Items)
            RenderItem(item, SeriesContext.Canvas, PaneContext.ToX(value.Moment), PaneContext.ToY(item.Value));
    }

    /// <summary>
    /// Calculates the minimum and maximum values from the multi-value data for chart scaling.
    /// </summary>
    /// <param name="values">The collection of multi-value items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<TM> values)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var value in values)
        {
            min = Math.Min(min, value.Items.Min(x => x.Value));
            max = Math.Max(max, value.Items.Max(x => x.Value));
        }

        return (min, max);
    }
}
