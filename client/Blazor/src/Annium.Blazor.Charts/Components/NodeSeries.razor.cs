using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class NodeSeries<T> : SeriesBase<T>, ILogSubject<NodeSeries<T>>
    where T : IPointValue
{
    [Parameter, EditorRequired]
    public Action<Canvas, int, int> RenderItem { get; set; } = delegate { };

    [Inject]
    public ILogger<NodeSeries<T>> Logger { get; set; } = default!;

    protected override void Render(IReadOnlyList<T> items)
    {
        this.Log().Trace($"render {items.Count} points");
        if (items.Count <= 1)
            return;

        var (min, max) = GetBounds(items);

        this.Log().Trace($"render in {min} - {max}");
        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(Source, min, max))
            return;

        var ctx = SeriesContext.Canvas;

        ctx.Save();

        foreach (var item in items)
            RenderItem(ctx, PaneContext.ToX(item.Moment), PaneContext.ToY(item.Value));

        ctx.Restore();
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
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