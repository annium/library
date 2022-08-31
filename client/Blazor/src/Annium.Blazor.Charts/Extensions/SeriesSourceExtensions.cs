using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;

namespace Annium.Blazor.Charts.Extensions;

public static class SeriesSourceExtensions
{
    public static Action RenderTo<TData>(
        this ISeriesSource<TData> source,
        IChartContext chartContext,
        Action<IReadOnlyList<TData>> render
    )
        where TData : ITimeSeries
    {
        void Draw()
        {
            var (start, end) = chartContext.Range;

            if (source.GetItems(start, end, out var data))
                render(data);
            else if (!source.IsLoading)
                source.LoadItems(start, end, Draw);
        }

        return chartContext.OnUpdate(Draw);
    }
}