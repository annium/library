using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Blazor component for rendering candlestick charts with configurable colors for up, down, and stale candles.
/// </summary>
/// <typeparam name="T">The type implementing ICandle interface representing candlestick data.</typeparam>
public partial class CandleSeries<T> : SeriesBase<T>, ILogSubject
    where T : ICandle
{
    /// <summary>
    /// Gets or sets the color used for up candles (where close is greater than open).
    /// </summary>
    [Parameter]
    public string UpColor { get; set; } = "#51A39A";

    /// <summary>
    /// Gets or sets the color used for down candles (where close is less than open).
    /// </summary>
    [Parameter]
    public string DownColor { get; set; } = "#DD5E56";

    /// <summary>
    /// Gets or sets the color used for stale candles (where close is equal to open).
    /// </summary>
    [Parameter]
    public string StaleColor { get; set; } = "#999999";

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets the minimum number of values required to render the series.
    /// </summary>
    protected override int MinValuesToRender => 1;

    /// <summary>
    /// Renders the candlestick values on the canvas.
    /// </summary>
    /// <param name="values">The collection of candle values to render.</param>
    protected override void RenderValues(IReadOnlyList<T> values)
    {
        var width = GetWidth();
        if (width == 1)
            RenderLineCandles(SeriesContext.Canvas, values);
        else
            RenderNormalCandles(SeriesContext.Canvas, values, ((double)width / 2).CeilInt32(), width);
    }

    /// <summary>
    /// Renders candles as simple lines when the width is 1 pixel.
    /// </summary>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="values">The collection of candle values to render.</param>
    private void RenderLineCandles(Canvas ctx, IReadOnlyList<T> values)
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

    /// <summary>
    /// Renders candles with full body and wick representation when width is greater than 1 pixel.
    /// </summary>
    /// <param name="ctx">The canvas context to render on.</param>
    /// <param name="values">The collection of candle values to render.</param>
    /// <param name="offset">The horizontal offset for centering the candle body.</param>
    /// <param name="width">The width of the candle body.</param>
    private void RenderNormalCandles(Canvas ctx, IReadOnlyList<T> values, int offset, int width)
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

    /// <summary>
    /// Calculates the minimum and maximum values from the candle data for chart scaling.
    /// </summary>
    /// <param name="items">The collection of candle items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
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

    /// <summary>
    /// Calculates the optimal width for rendering candles based on the chart resolution.
    /// </summary>
    /// <returns>The calculated width in pixels, always an odd number for proper centering.</returns>
    private int GetWidth()
    {
        var width = (ChartContext.PxPerResolution / 1.3d).RoundInt32().Above(1);

        return width % 2 == 1 ? width : width - 1;
    }
}
