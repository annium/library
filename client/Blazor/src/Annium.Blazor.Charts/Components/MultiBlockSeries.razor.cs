using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using OneOf;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Renders multiple block series where blocks are drawn between related range items across consecutive data points
/// </summary>
/// <typeparam name="TM">The multi-value type that implements IMultiValue</typeparam>
/// <typeparam name="TI">The range item type that implements IRangeItem</typeparam>
public partial class MultiBlockSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IRangeItem
{
    /// <summary>
    /// Gets or sets the function that determines if two range items are related and should be connected with a block
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TI, TI, bool> IsRelated { get; set; } =
        delegate
        {
            return false;
        };

    /// <summary>
    /// Gets or sets the color for block items, either as a static color string or a function that returns color based on the item
    /// </summary>
    [Parameter, EditorRequired]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; }

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
    /// Renders the block values on the canvas by drawing blocks between related items
    /// </summary>
    /// <param name="items">The collection of multi-value items to render</param>
    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        var ctx = SeriesContext.Canvas;

        for (var i = 0; i < items.Count - 1; i++)
            RenderBlock(ctx, items[i], items[i + 1]);
    }

    /// <summary>
    /// Renders blocks between two consecutive multi-value items for related range items
    /// </summary>
    /// <param name="ctx">The canvas context for rendering</param>
    /// <param name="a">The first multi-value item</param>
    /// <param name="b">The second multi-value item</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderBlock(Canvas ctx, TM a, TM b)
    {
        var left = PaneContext.ToX(a.Moment);
        var right = PaneContext.ToX(b.Moment);
        var width = right - left;

        foreach (var range in a.Items.Where(ai => b.Items.Any(bi => IsRelated(ai, bi))))
        {
            ctx.FillStyle = ItemColor.Match(value => value, get => get(range));
            var low = PaneContext.ToY(range.Low);
            var high = PaneContext.ToY(range.High);

            ctx.FillRect(left, high, width, low - high);
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
}
