using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using NodaTime;
using OneOf;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Renders multiple range series where each data point can contain multiple range items
/// </summary>
/// <typeparam name="TM">The multi-value type that implements IMultiValue</typeparam>
/// <typeparam name="TI">The range item type that implements IRangeItem</typeparam>
public partial class MultiRangeSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IRangeItem
{
    /// <summary>
    /// Gets or sets the color for range items, either as a static color string or a function that returns color based on the item
    /// </summary>
    [Parameter, EditorRequired]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; }

    /// <summary>
    /// Gets or sets whether the range items should be centered on their timestamp
    /// </summary>
    [Parameter]
    public bool Centered { get; set; }

    /// <summary>
    /// Gets or sets whether the last range item should continue to the end of the chart view
    /// </summary>
    [Parameter]
    public bool ContinueLast { get; set; }

    /// <summary>
    /// Gets or sets the logger instance for this component
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets the minimum number of values required to render this series
    /// </summary>
    protected override int MinValuesToRender => 1;

    /// <summary>
    /// Renders the range values on the canvas
    /// </summary>
    /// <param name="items">The collection of multi-value items to render</param>
    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        var width = GetWidth();
        var offset = Centered
            ? width == 1
                ? 0
                : ((double)width / 2).CeilInt32()
            : 0;
        var lastMoment = ContinueLast
            ? ChartContext.View.End
            : ChartContext.FromX(ChartContext.ToX(items[^1].Moment) + width);
        var ctx = SeriesContext.Canvas;

        for (var i = 0; i < items.Count - 1; i++)
            RenderItem(ctx, items[i], items[i + 1].Moment, offset);
        RenderItem(ctx, items[^1], lastMoment, offset);
    }

    /// <summary>
    /// Renders a single multi-value item with its range items
    /// </summary>
    /// <param name="ctx">The canvas context for rendering</param>
    /// <param name="item">The multi-value item to render</param>
    /// <param name="to">The end timestamp for the item</param>
    /// <param name="offset">The horizontal offset for centering</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderItem(Canvas ctx, TM item, Instant to, int offset)
    {
        var left = PaneContext.ToX(item.Moment);
        var right = PaneContext.ToX(to);
        var width = right - left;

        foreach (var range in item.Items)
        {
            ctx.FillStyle = ItemColor.Match(value => value, get => get(range));
            var low = PaneContext.ToY(range.Low);
            var high = PaneContext.ToY(range.High);

            ctx.FillRect(left - offset, high, width, low - high);
        }
    }

    /// <summary>
    /// Calculates the minimum and maximum bounds for the given items
    /// </summary>
    /// <param name="items">The collection of multi-value items to analyze</param>
    /// <returns>A tuple containing the minimum and maximum values</returns>
    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<TM> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Items.Min(x => x.Low));
            max = Math.Max(max, item.Items.Max(x => x.High));
        }

        return (min, max);
    }

    /// <summary>
    /// Calculates the optimal width for rendering range items
    /// </summary>
    /// <returns>The width in pixels, adjusted to be odd for proper centering</returns>
    private int GetWidth()
    {
        var width = ChartContext.PxPerResolution.Above(1);

        return width % 2 == 1 ? width : width - 1;
    }
}
