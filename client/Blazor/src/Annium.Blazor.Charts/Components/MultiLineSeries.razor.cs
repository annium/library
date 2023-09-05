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

public partial class MultiLineSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    [Parameter, EditorRequired]
    public Func<TI, TI, bool> IsRelated { get; set; } = delegate { return false; };

    [Parameter]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; } = "black";

    [Parameter]
    public OneOf<int, Func<TI, int>> Width { get; set; } = 1;

    [Parameter]
    public OneOf<int, Func<TI, int>> Radius { get; set; } = 2;

    [Inject]
    public ILogger Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderValueItemPoints(Canvas ctx, TM value)
    {
        var x = PaneContext.ToX(value.Moment);

        foreach (var item in value.Items)
        {
            var width = Width.Match(x => x, x => x(item));
            var radius = Radius.Match(x => x, x => x(item));
            var color = ItemColor.Match(x => x, x => x(item));
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

    private sealed record LastItem(Instant Moment, TI Item)
    {
        public Instant Moment { get; private set; } = Moment;
        public TI Item { get; private set; } = Item;

        public void Update(Instant moment, TI item)
        {
            Moment = moment;
            Item = item;
        }
    }
}