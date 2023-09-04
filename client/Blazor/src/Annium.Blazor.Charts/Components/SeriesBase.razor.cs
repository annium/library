using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public abstract partial class SeriesBase<T> : IAsyncDisposable
    where T : ITimeSeries
{
    [Parameter, EditorRequired]
    public ISeriesSource<T> Source { get; set; } = default!;

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = default!;

    protected abstract int MinValuesToRender { get; }

    private DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source && source != default!)
        {
            _disposable.DisposeAndReset();
            _disposable += PaneContext.RegisterSource(Source);
            _disposable += Source.RenderTo(ChartContext, Render);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += PaneContext.RegisterSource(Source);
        _disposable += Source.RenderTo(ChartContext, Render);
    }

    private void Render(IReadOnlyList<T> values)
    {
        if (values.Count < MinValuesToRender)
        {
            PaneContext.AdjustRange(Source, decimal.MinValue, decimal.MaxValue);
            return;
        }

        var (min, max) = GetBounds(values);

        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(Source, min, max))
            return;

        var ctx = SeriesContext.Canvas;

        ctx.Save();

        RenderValues(values);

        ctx.Restore();
    }

    protected abstract void RenderValues(IReadOnlyList<T> values);

    protected abstract (decimal min, decimal max) GetBounds(IReadOnlyList<T> items);

    public ValueTask DisposeAsync()
    {
        _disposable.Dispose();

        return ValueTask.CompletedTask;
    }
}