using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class SeriesSourceFactory : ISeriesSourceFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public SeriesSourceFactory(
        ILoggerFactory loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
    }

    public ISeriesSource<TData> Create<TData>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> load,
        SeriesSourceCacheOptions cacheOptions
    )
        where TData : ITimeSeries
        => Create(resolution, load, new SeriesSourceOptions(1L, 3L, 8L), cacheOptions);

    public ISeriesSource<T> Create<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options,
        SeriesSourceCacheOptions cacheOptions
    )
        where T : ITimeSeries
    {
        var cache = new SeriesSourceCache<T>(resolution, cacheOptions);
        var logger = _loggerFactory.Get<LoadingSeriesSource<T>>();

        return new LoadingSeriesSource<T>(cache, resolution, load, options, logger);
    }

    public ISeriesSource<TData> Create<TSource, TData>(
        ISeriesSource<TSource> source,
        Func<TSource, TData?> getValue
    )
        where TSource : ITimeSeries
        where TData : ITimeSeries
    {
        return new DependentSeriesSource<TSource, TData>(source, getValue, _loggerFactory.Get<DependentSeriesSource<TSource, TData>>());
    }
}