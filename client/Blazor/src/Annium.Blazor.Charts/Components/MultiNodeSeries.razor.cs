using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class MultiNodeSeries<TM, TI> : SeriesBase<TM>, ILogSubject<MultiNodeSeries<TM, TI>>
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    [Parameter, EditorRequired]
    public Action<Canvas, int, int> RenderItem { get; set; } = delegate { };

    [Inject]
    public ILogger<MultiNodeSeries<TM, TI>> Logger { get; set; } = default!;

    protected override void Render(IReadOnlyList<TM> values)
    {
        this.Log().Trace($"render {values.Count} points");
        if (values.Count <= 1)
            return;

        var (min, max) = GetBounds(values);

        this.Log().Trace($"render in {min} - {max}");
        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(Source, min, max))
            return;

        var ctx = SeriesContext.Canvas;

        ctx.Save();

        foreach (var value in values)
        foreach (var item in value.Values)
            RenderItem(ctx, PaneContext.ToX(value.Moment), PaneContext.ToY(item.Value));

        ctx.Restore();
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<TM> values)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var value in values)
        {
            min = Math.Min(min, value.Values.Min(x => x.Value));
            max = Math.Max(max, value.Values.Max(x => x.Value));
        }

        return (min, max);
    }
}