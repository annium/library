using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using NodaTime;

namespace Annium.Blazor.Charts.Data;

public interface ISeriesSourceFactory
{
    ISeriesSource<TData> Create<TData>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> load
    )
        where TData : ITimeSeries;

    ISeriesSource<TData> Create<TSource, TData>(
        ISeriesSource<TSource> source,
        Func<TSource, TData> getValue
    )
        where TSource : ITimeSeries
        where TData : ITimeSeries;
}