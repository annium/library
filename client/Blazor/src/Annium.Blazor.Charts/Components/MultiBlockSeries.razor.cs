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

public partial class MultiBlockSeries<TM, TI> : SeriesBase<TM>, ILogSubject
    where TM : IMultiValue<TI>
    where TI : IRangeItem
{
    [Parameter, EditorRequired]
    public Func<TI, TI, bool> IsRelated { get; set; } = delegate { return false; };

    [Parameter, EditorRequired]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; }

    [Inject]
    public ILogger Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        var ctx = SeriesContext.Canvas;

        for (var i = 0; i < items.Count - 1; i++)
            RenderBlock(ctx, items[i], items[i + 1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderBlock(
        Canvas ctx,
        TM a,
        TM b
    )
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