using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Logging;
using Annium.NodaTime.Extensions;

namespace Annium.Blazor.Charts.Extensions;

/// <summary>
/// Extension methods for ISeriesSource to provide rendering and subscription utilities
/// </summary>
public static class SeriesSourceExtensions
{
    /// <summary>
    /// Sets up automatic rendering of series data when the chart context updates or data is loaded
    /// </summary>
    /// <typeparam name="T">The type of time series data</typeparam>
    /// <param name="source">The series source to render from</param>
    /// <param name="chartContext">The chart context that provides view information</param>
    /// <param name="render">The action to call with the loaded data for rendering</param>
    /// <returns>A disposable that can be used to unsubscribe from updates</returns>
    public static IDisposable RenderTo<T>(
        this ISeriesSource<T> source,
        IChartContext chartContext,
        Action<IReadOnlyList<T>> render
    )
        where T : ITimeSeries
    {
        void Draw()
        {
            var start = chartContext.View.Start.FloorTo(chartContext.Resolution);
            var end = chartContext.View.End.CeilTo(chartContext.Resolution);

            if (source.GetItems(start, end, out var data))
            {
                render(data);
            }
            else if (!source.IsLoading)
            {
                source.LoadItems(start, end);
            }
        }

        var disposable = Disposable.Box(VoidLogger.Instance);
        disposable += chartContext.OnUpdate(Draw);
        source.Loaded += Draw;
        disposable += () => source.Loaded -= Draw;

        return disposable;
    }
}
