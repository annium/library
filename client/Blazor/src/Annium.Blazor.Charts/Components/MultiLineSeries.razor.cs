using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using OneOf;

namespace Annium.Blazor.Charts.Components;

public partial class MultiLineSeries<TM, TI> : SeriesBase<TM>, ILogSubject<MultiLineSeries<TM, TI>>
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
    public ILogger<MultiLineSeries<TM, TI>> Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<TM> items)
    {
        for (var i = 0; i < items.Count - 1; i++)
            RenderItem(SeriesContext.Canvas, items[i], items[i + 1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderItem(Canvas ctx, TM a, TM b)
    {
        var left = PaneContext.ToX(a.Moment);
        var right = PaneContext.ToX(b.Moment);

        foreach (var av in a.Values)
        {
            var width = Width.Match(x => x, x => x(av));
            var radius = Radius.Match(x => x, x => x(av));
            var color = ItemColor.Match(x => x, x => x(av));
            var y = PaneContext.ToY(av.Value);

            ctx.StrokeStyle = color;
            ctx.LineWidth = width;

            ctx.BeginPath();
            ctx.MoveTo(left, y);
            ctx.Arc(left, y, radius, 0, (float) (2 * Math.PI), false);
            ctx.FillStyle = color;
            ctx.Fill();
            ctx.ClosePath();

            foreach (var bv in b.Values)
            {
                if (!IsRelated(bv, av))
                    continue;

                ctx.BeginPath();
                ctx.MoveTo(left, y);
                ctx.LineTo(right, PaneContext.ToY(bv.Value));
                ctx.Stroke();
                ctx.ClosePath();
            }
        }
    }

    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<TM> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Values.Min(x => x.Value));
            max = Math.Max(max, item.Values.Max(x => x.Value));
        }

        return (min, max);
    }
}