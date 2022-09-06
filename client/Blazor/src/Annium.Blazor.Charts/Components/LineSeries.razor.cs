using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class LineSeries<T> : ILogSubject<LineSeries<T>>, IAsyncDisposable
    where T : PlainValue
{
    [Parameter, EditorRequired]
    public ISeriesSource<T> Source { get; set; } = default!;

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

    [Inject]
    public ILogger<LineSeries<T>> Logger { get; set; } = default!;

    private Action _unregisterSource = delegate { };
    private Action _unregisterRender = delegate { };

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source)
        {
            this.Log().Trace("update {oldSource} -> {newSource}", source.GetFullId(), Source.GetFullId());
            _unregisterSource();
            _unregisterSource = PaneContext.RegisterSource(Source);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        this.Log().Trace("register Draw");
        _unregisterRender = Source.RenderTo(ChartContext, Render);
    }

    private void Render(IReadOnlyList<T> items)
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

        // first point
        {
            var x = PaneContext.ToX(items[0].Moment);
            var y = PaneContext.ToY(items[0].Value);

            ctx.MoveTo(x, y);
        }

        for (var i = 1; i < items.Count; i++)
        {
            var item = items[i];
            var x = PaneContext.ToX(item.Moment);
            var y = PaneContext.ToY(item.Value);

            ctx.LineTo(x, y);
        }

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

    public ValueTask DisposeAsync()
    {
        _unregisterSource();
        _unregisterRender();

        return ValueTask.CompletedTask;
    }
}