using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Blazor.Charts.Internal.Data.Sources;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSourceFactory
{
    ISeriesSource<T> Create<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceCacheOptions cacheOptions
    )
        where T : ITimeSeries;

    ISeriesSource<T> Create<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options,
        SeriesSourceCacheOptions cacheOptions
    )
        where T : ITimeSeries;

    ISeriesSource<TD> Create<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues,
        SeriesSourceCacheOptions cacheOptions
    )
        where TS : ITimeSeries
        where TD : ITimeSeries;
}