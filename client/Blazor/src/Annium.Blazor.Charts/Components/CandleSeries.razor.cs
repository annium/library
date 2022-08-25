using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class CandleSeries : ILogSubject<CandleSeries>, IAsyncDisposable
{
    [Parameter, EditorRequired]
    public ISeriesSource<ICandle> Source { get; set; } = default!;

    [Parameter]
    public string UpColor { get; set; } = "#51A39A";

    [Parameter]
    public string DownColor { get; set; } = "#DD5E56";

    [Parameter]
    public string StaleColor { get; set; } = "#999999";

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    internal ISeriesContext SeriesContext { get; set; } = default!;

    [Inject]
    public ILogger<CandleSeries> Logger { get; set; } = default!;

    private Action _unregisterSource = delegate { };
    private Action _unregisterDraw = delegate { };

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source)
        {
            this.Log().Debug("update source registration");
            _unregisterSource();
            _unregisterSource = PaneContext.RegisterSource(Source);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        this.Log().Debug("register Draw");
        _unregisterDraw = ChartContext.OnUpdate(Draw);
    }

    private void Draw()
    {
        var (start, end) = ChartContext.Range;

        if (Source.GetItems(start, end, out var data))
            Render(data);
        else if (!Source.IsLoading)
            Source.LoadItems(start, end, Draw);
    }

    private void Render(IReadOnlyList<ICandle> items)
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
        var offset = width == 1 ? 0 : ((double) width / 2).FloorInt32();
        var start = ChartContext.View.Start;
        var msPerPx = ChartContext.MsPerPx;
        var rangeUp = PaneContext.View.End;
        var dpx = PaneContext.DotPerPx;
        var ctx = SeriesContext.Canvas;

        ctx.Save();

        if (width == 1)
        {
            ctx.FillStyle = UpColor;
            foreach (var item in items.Where(x => x.Open < x.Close))
            {
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
            }

            ctx.FillStyle = DownColor;
            foreach (var item in items.Where(x => x.Open > x.Close))
            {
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
            }

            ctx.FillStyle = StaleColor;
            foreach (var item in items.Where(x => x.Open == x.Close))
            {
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
            }
        }
        else
        {
            ctx.FillStyle = UpColor;
            foreach (var item in items.Where(x => x.Open < x.Close))
            {
                var open = ((rangeUp - item.Open) / dpx).RoundInt32();
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();
                var close = ((rangeUp - item.Close) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
                ctx.FillRect(x - offset, close, width, open - close);
            }

            ctx.FillStyle = DownColor;
            foreach (var item in items.Where(x => x.Open > x.Close))
            {
                var open = ((rangeUp - item.Open) / dpx).RoundInt32();
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();
                var close = ((rangeUp - item.Close) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
                ctx.FillRect(x - offset, open, width, close - open);
            }

            ctx.FillStyle = StaleColor;
            foreach (var item in items.Where(x => x.Open == x.Close))
            {
                var open = ((rangeUp - item.Open) / dpx).RoundInt32();
                var high = ((rangeUp - item.High) / dpx).RoundInt32();
                var low = ((rangeUp - item.Low) / dpx).RoundInt32();

                var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32();

                ctx.FillRect(x, high, 1, low - high);
                ctx.FillRect(x - offset, open, width, 1);
            }
        }

        ctx.Restore();
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<ICandle> items)
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

    public ValueTask DisposeAsync()
    {
        _unregisterSource();
        _unregisterDraw();

        return ValueTask.CompletedTask;
    }
}