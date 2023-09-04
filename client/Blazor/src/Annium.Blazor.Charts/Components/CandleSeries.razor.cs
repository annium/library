using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;


namespace Annium.Blazor.Charts.Components;

public partial class CandleSeries<T> : SeriesBase<T>, ILogSubject
    where T : ICandle
{
    [Parameter]
    public string UpColor { get; set; } = "#51A39A";

    [Parameter]
    public string DownColor { get; set; } = "#DD5E56";

    [Parameter]
    public string StaleColor { get; set; } = "#999999";

    [Inject]
    public ILogger Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<T> values)
    {
        var width = GetWidth();
        if (width == 1)
            RenderLineCandles(SeriesContext.Canvas, values);
        else
            RenderNormalCandles(SeriesContext.Canvas, values, ((double) width / 2).CeilInt32(), width);
    }

    private void RenderLineCandles(
        Canvas ctx,
        IReadOnlyList<T> values
    )
    {
        ctx.FillStyle = UpColor;
        foreach (var item in values.Where(x => x.Open < x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }

        ctx.FillStyle = DownColor;
        foreach (var item in values.Where(x => x.Open > x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }

        ctx.FillStyle = StaleColor;
        foreach (var item in values.Where(x => x.Open == x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }
    }

    private void RenderNormalCandles(
        Canvas ctx,
        IReadOnlyList<T> values,
        int offset,
        int width
    )
    {
        ctx.FillStyle = UpColor;
        foreach (var item in values.Where(x => x.Open < x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var open = PaneContext.ToY(item.Open);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);
            var close = PaneContext.ToY(item.Close);

            ctx.FillRect(x - 1, high, 1, low - high);
            ctx.FillRect(x - offset, close, width, open - close);
        }

        ctx.FillStyle = DownColor;
        foreach (var item in values.Where(x => x.Open > x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var open = PaneContext.ToY(item.Open);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);
            var close = PaneContext.ToY(item.Close);

            ctx.FillRect(x - 1, high, 1, low - high);
            ctx.FillRect(x - offset, open, width, close - open);
        }

        ctx.FillStyle = StaleColor;
        foreach (var item in values.Where(x => x.Open == x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var open = PaneContext.ToY(item.Open);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x - 1, high, 1, low - high);
            ctx.FillRect(x - offset, open, width, 1);
        }
    }

    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Low);
            max = Math.Max(max, item.High);
        }

        return (min, max);
    }

    private int GetWidth()
    {
        var width = (ChartContext.PxPerResolution / 1.3d).RoundInt32().Above(1);

        return width % 2 == 1 ? width : width - 1;
    }
}