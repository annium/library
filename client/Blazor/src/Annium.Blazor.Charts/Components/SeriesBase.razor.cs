using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
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

    private Action _unregisterSource = delegate { };
    private Action _unregisterRender = delegate { };

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source)
        {
            _unregisterSource();
            _unregisterSource = PaneContext.RegisterSource(Source);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _unregisterRender = Source.RenderTo(ChartContext, Render);
    }

    protected abstract void Render(IReadOnlyList<T> items);

    public ValueTask DisposeAsync()
    {
        _unregisterSource();
        _unregisterRender();

        return ValueTask.CompletedTask;
    }
}