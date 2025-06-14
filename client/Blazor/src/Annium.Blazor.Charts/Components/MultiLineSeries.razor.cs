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
/// Blazor component for rendering multiple line series with dynamic styling and relationship tracking between items.
/// </summary>
/// <typeparam name="TM">The type implementing IMultiValue interface representing multi-value data points.</typeparam>
/// <typeparam name="TI">The type implementing IPointItem interface representing individual point items.</typeparam>
public partial class MultiLineSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    /// <summary>
    /// Gets or sets the function that determines if two point items are related and should be connected by a line.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TI, TI, bool> IsRelated { get; set; } =
        delegate
        {
            return false;
        };

    /// <summary>
    /// Gets or sets the color for items, either as a static color or a function that returns color based on the item.
    /// </summary>
    [Parameter]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; } = "black";

    /// <summary>
    /// Gets or sets the line width, either as a static value or a function that returns width based on the item.
    /// </summary>
    [Parameter]
    public OneOf<int, Func<TI, int>> Width { get; set; } = 1;

    /// <summary>
    /// Gets or sets the point radius, either as a static value or a function that returns radius based on the item.
    /// </summary>
    [Parameter]
    public OneOf<int, Func<TI, int>> Radius { get; set; } = 2;

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
    /// Renders the multi-line series values on the canvas.
    /// </summary>
    /// <param name="items">The collection of multi-value items to render.</param>
    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        var ctx = SeriesContext.Canvas;
        var first = items[0];
        RenderValueItemPoints(ctx, first);
        if (items.Count == 1)
            return;

        var lastItems = first.Items.Select(x => new LastItem(first.Moment, x)).ToList();

        for (var i = 1; i < items.Count; i++)
            RenderValue(SeriesContext.Canvas, items[i], lastItems);
    }

    /// <summary>
    /// Renders a single multi-value item and connects related items with lines.
    /// </summary>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="value">The multi-value item to render.</param>
    /// <param name="lastItems">The collection of previously rendered items for line connection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderValue(Canvas ctx, TM value, List<LastItem> lastItems)
    {
        RenderValueItemPoints(ctx, value);

        var right = PaneContext.ToX(value.Moment);

        foreach (var item in value.Items)
        {
            var index = lastItems.FindIndex(x => IsRelated(item, x.Item));
            var lastItem = new LastItem(value.Moment, item);
            if (index < 0)
            {
                lastItems.Add(lastItem);
                continue;
            }

            var prev = lastItems[index];
            lastItems[index] = lastItem;

            ctx.StrokeStyle = ItemColor.Match(x => x, x => x(item));
            ctx.BeginPath();
            ctx.MoveTo(PaneContext.ToX(prev.Moment), PaneContext.ToY(prev.Item.Value));
            ctx.LineTo(right, PaneContext.ToY(item.Value));
            ctx.Stroke();
            ctx.ClosePath();
        }
    }

    /// <summary>
    /// Renders the individual point items within a multi-value item.
    /// </summary>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="value">The multi-value item containing points to render.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderValueItemPoints(Canvas ctx, TM value)
    {
        var x = PaneContext.ToX(value.Moment);

        foreach (var item in value.Items)
        {
            var width = Width.Match(v => v, v => v(item));
            var radius = Radius.Match(v => v, v => v(item));
            var color = ItemColor.Match(v => v, v => v(item));
            var y = PaneContext.ToY(item.Value);

            ctx.StrokeStyle = color;
            ctx.LineWidth = width;

            ctx.BeginPath();
            ctx.MoveTo(x, y);
            ctx.Arc(x, y, radius, 0, (float)(2 * Math.PI), false);
            ctx.FillStyle = color;
            ctx.Fill();
            ctx.ClosePath();
        }
    }

    /// <summary>
    /// Calculates the minimum and maximum values from the multi-value data for chart scaling.
    /// </summary>
    /// <param name="items">The collection of multi-value items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<TM> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Items.Min(x => x.Value));
            max = Math.Max(max, item.Items.Max(x => x.Value));
        }

        return (min, max);
    }

    /// <summary>
    /// Represents the last rendered item for tracking line connections between related items.
    /// </summary>
    /// <param name="Moment">The time instant of the item.</param>
    /// <param name="Item">The point item data.</param>
    private sealed record LastItem(Instant Moment, TI Item)
    {
        /// <summary>
        /// Gets the time instant of the item.
        /// </summary>
        public Instant Moment { get; private set; } = Moment;

        /// <summary>
        /// Gets the point item data.
        /// </summary>
        public TI Item { get; private set; } = Item;

        /// <summary>
        /// Updates the last item with new moment and item data.
        /// </summary>
        /// <param name="moment">The new time instant.</param>
        /// <param name="item">The new point item data.</param>
        public void Update(Instant moment, TI item)
        {
            Moment = moment;
            Item = item;
        }
    }
}
