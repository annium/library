using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class LineSeries<T> : SeriesBase<T>, ILogSubject
    where T : IPointValue
{
    [Parameter]
    public string Color { get; set; } = "black";

    [Parameter]
    public int Width { get; set; } = 1;

    [Parameter]
    public int[]? Dash { get; set; }

    [Inject]
    public ILogger Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<T> items)
    {
        var ctx = SeriesContext.Canvas;

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

        ctx.LineTo((float)PaneContext.Rect.Width, PaneContext.ToY(items[^1].Value));

        ctx.Stroke();
        ctx.ClosePath();
    }

    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
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
