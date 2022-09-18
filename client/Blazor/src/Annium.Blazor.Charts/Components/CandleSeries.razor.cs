using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class CandleSeries<T> : SeriesBase<T>, ILogSubject<CandleSeries<T>>
    where T : ICandle
{
    [Parameter]
    public string UpColor { get; set; } = "#51A39A";

    [Parameter]
    public string DownColor { get; set; } = "#DD5E56";

    [Parameter]
    public string StaleColor { get; set; } = "#999999";

    [Inject]
    public ILogger<CandleSeries<T>> Logger { get; set; } = default!;

    protected override void Render(IReadOnlyList<T> items)
    {
        if (items.Count == 0)
        {
            this.Log().Trace("No candles to render");
            return;
        }

        var (min, max) = GetBounds(items);

        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(Source, min, max))
        {
            this.Log().Trace("adjusted to range {min} - {max}, wait for redraw", min, max);
            return;
        }

        this.Log().Trace("render {count} in range {min} - {max}", items.Count, min, max);
        var width = GetWidth();
        var ctx = SeriesContext.Canvas;

        ctx.Save();

        if (width == 1)
            RenderLineCandles(ctx, items);
        else
            RenderNormalCandles(ctx, items, ((double) width / 2).CeilInt32(), width);

        ctx.Restore();
    }

    private void RenderLineCandles(
        Canvas ctx,
        IReadOnlyList<T> items
    )
    {
        ctx.FillStyle = UpColor;
        foreach (var item in items.Where(x => x.Open < x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }

        ctx.FillStyle = DownColor;
        foreach (var item in items.Where(x => x.Open > x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }

        ctx.FillStyle = StaleColor;
        foreach (var item in items.Where(x => x.Open == x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x, high, 1, low - high);
        }
    }

    private void RenderNormalCandles(
        Canvas ctx,
        IReadOnlyList<T> items,
        int offset,
        int width
    )
    {
        ctx.FillStyle = UpColor;
        foreach (var item in items.Where(x => x.Open < x.Close))
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
        foreach (var item in items.Where(x => x.Open > x.Close))
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
        foreach (var item in items.Where(x => x.Open == x.Close))
        {
            var x = PaneContext.ToX(item.Moment);
            var open = PaneContext.ToY(item.Open);
            var high = PaneContext.ToY(item.High);
            var low = PaneContext.ToY(item.Low);

            ctx.FillRect(x - 1, high, 1, low - high);
            ctx.FillRect(x - offset, open, width, 1);
        }
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
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