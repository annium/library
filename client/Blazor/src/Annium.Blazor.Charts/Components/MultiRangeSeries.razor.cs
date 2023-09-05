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

public partial class MultiRangeSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IRangeItem
{
    [Parameter, EditorRequired]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; }

    [Parameter]
    public bool Centered { get; set; }

    [Parameter]
    public bool ContinueLast { get; set; }

    [Inject]
    public ILogger Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        var width = GetWidth();
        var offset = Centered ? width == 1 ? 0 : ((double)width / 2).CeilInt32() : 0;
        var lastMoment = ContinueLast ? ChartContext.View.End : ChartContext.FromX(ChartContext.ToX(items[^1].Moment) + width);
        var ctx = SeriesContext.Canvas;

        for (var i = 0; i < items.Count - 1; i++)
            RenderItem(ctx, items[i], items[i + 1].Moment, offset);
        RenderItem(ctx, items[^1], lastMoment, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderItem(
        Canvas ctx,
        TM item,
        Instant to,
        int offset
    )
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

    private int GetWidth()
    {
        var width = ChartContext.PxPerResolution.Above(1);

        return width % 2 == 1 ? width : width - 1;
    }
}