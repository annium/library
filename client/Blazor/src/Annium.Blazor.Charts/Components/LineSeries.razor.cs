using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class LineSeries : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public ISeriesSource<IValue> Source { get; set; } = default!;

    [Parameter]
    public string Color { get; set; } = "black";

    [Parameter]
    public int Width { get; set; } = 1;

    [Parameter]
    public int[]? Dash { get; set; }

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    internal ISeriesContext SeriesContext { get; set; } = default!;

    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += PaneContext.RegisterSource(Source);
        _disposable += ChartContext.OnUpdate(Draw);
    }

    private void Draw()
    {
        var (start, end) = ChartContext.Range;

        if (Source.GetData(start, end, out var data))
            Render(data);
        else if (!Source.IsLoading)
            Source.LoadData(start, end, Draw);
    }

    private void Render(IReadOnlyList<IValue> items)
    {
        if (items.Count <= 1)
            return;

        var (min, max) = GetBounds(items);

        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(min, max))
            return;

        var start = ChartContext.View.Start;
        var msPerPx = ChartContext.MsPerPx;
        var rangeUp = PaneContext.View.End;
        var dpx = PaneContext.DotPerPx;
        var ctx = SeriesContext.Canvas;

        ctx.Save();

        ctx.StrokeStyle = Color;
        ctx.LineWidth = Width;
        if (Dash is not null)
            ctx.LineDash = Dash;

        ctx.BeginPath();

        // first point
        {
            var x = ((items[0].Moment - start).TotalMilliseconds.FloorInt64() / (decimal)msPerPx).FloorInt32();
            var y = ((rangeUp - items[0].Value) / dpx).FloorInt32();

            ctx.MoveTo(x, y);
        }

        for (var i = 1; i < items.Count; i++)
        {
            var item = items[i];
            var x = ((item.Moment - start).TotalMilliseconds.FloorInt64() / (decimal)msPerPx).FloorInt32();
            var y = ((rangeUp - item.Value) / dpx).FloorInt32();

            ctx.LineTo(x, y);
        }

        ctx.Stroke();
        ctx.ClosePath();

        ctx.Restore();
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<IValue> items)
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

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}