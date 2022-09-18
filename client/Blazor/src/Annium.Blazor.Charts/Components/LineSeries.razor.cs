using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Models;
using Annium.Blazor.Charts.Extensions;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class LineSeries<T> : SeriesBase<T>, ILogSubject<LineSeries<T>>
    where T : PointValue
{
    [Parameter]
    public string Color { get; set; } = "black";

    [Parameter]
    public int Width { get; set; } = 1;

    [Parameter]
    public int[]? Dash { get; set; }

    [Inject]
    public ILogger<LineSeries<T>> Logger { get; set; } = default!;

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

        ctx.StrokeStyle = Color;
        ctx.LineWidth = Width;
        if (Dash is not null)
            ctx.LineDash = Dash;

        ctx.BeginPath();

        ctx.MoveTo(0, PaneContext.ToY(items[0].Value));

        foreach (var item in items)
        {
            var x = PaneContext.ToX(item.Moment);
            var y = PaneContext.ToY(item.Value);

            ctx.LineTo(x, y);
        }

        ctx.LineTo((float) PaneContext.Rect.Width, PaneContext.ToY(items[^1].Value));

        ctx.Stroke();
        ctx.ClosePath();

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