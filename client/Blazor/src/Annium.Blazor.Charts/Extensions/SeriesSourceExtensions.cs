using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Logging;
using Annium.NodaTime.Extensions;

namespace Annium.Blazor.Charts.Extensions;

public static class SeriesSourceExtensions
{
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