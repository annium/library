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

/// <summary>
/// Base class for all chart series components that render time-based data.
/// </summary>
/// <typeparam name="T">The type of time series data this series handles.</typeparam>
public abstract partial class SeriesBase<T> : IAsyncDisposable
    where T : ITimeSeries
{
    /// <summary>
    /// Gets or sets the data source for this series.
    /// </summary>
    [Parameter, EditorRequired]
    public ISeriesSource<T> Source { get; set; } = null!;

    /// <summary>
    /// Gets or sets the chart context from the parent chart component.
    /// </summary>
    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the pane context from the parent pane component.
    /// </summary>
    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the series context for rendering operations.
    /// </summary>
    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = null!;

    /// <summary>
    /// Gets the minimum number of values required to render this series.
    /// </summary>
    protected abstract int MinValuesToRender { get; }

    /// <summary>
    /// Container for managing disposable resources.
    /// </summary>
    private DisposableBox _disposable = Disposable.Box(VoidLogger.Instance);

    /// <summary>
    /// Sets the parameters for the component and manages data source changes.
    /// </summary>
    /// <param name="parameters">The parameter values to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source && source != null!)
        {
            _disposable.DisposeAndReset();
            _disposable += PaneContext.RegisterSource(Source);
            _disposable += Source.RenderTo(ChartContext, Render);
        }
    }

    /// <summary>
    /// Called after the component has been rendered. Registers the data source on first render.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += PaneContext.RegisterSource(Source);
        _disposable += Source.RenderTo(ChartContext, Render);
    }

    /// <summary>
    /// Renders the series with the provided values.
    /// </summary>
    /// <param name="values">The data values to render.</param>
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

    /// <summary>
    /// Renders the actual data values. Must be implemented by derived classes.
    /// </summary>
    /// <param name="values">The data values to render.</param>
    protected abstract void RenderValues(IReadOnlyList<T> values);

    /// <summary>
    /// Gets the minimum and maximum bounds for the provided data items. Must be implemented by derived classes.
    /// </summary>
    /// <param name="items">The data items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    protected abstract (decimal min, decimal max) GetBounds(IReadOnlyList<T> items);

    /// <summary>
    /// Disposes of the component's resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        _disposable.Dispose();

        return ValueTask.CompletedTask;
    }
}
