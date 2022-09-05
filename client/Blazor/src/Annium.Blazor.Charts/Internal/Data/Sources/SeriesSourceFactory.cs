using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class SeriesSourceFactory : ISeriesSourceFactory
{
    private readonly SeriesSourceOptions _defaultSeriesSourceOptions = new(1L, 3L, 8L);
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
        => Create(resolution, load, _defaultSeriesSourceOptions, cacheOptions);

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

    public ISeriesSource<TD> Create<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues,
        SeriesSourceCacheOptions cacheOptions
    )
        where TS : ITimeSeries
        where TD : ITimeSeries
    {
        var cache = new SeriesSourceCache<TD>(source.Resolution, cacheOptions);
        var logger = _loggerFactory.Get<DependentSeriesSource<TS, TD>>();

        return new DependentSeriesSource<TS, TD>(source, cache, getValues, logger);
    }
}