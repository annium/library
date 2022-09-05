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

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load
    )
        where T : IComparable<T>, IComparable<Instant>
        => CreateUnchecked(resolution, load, _defaultSeriesSourceOptions);


    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load
    )
        where T : ITimeSeries, IComparable<T>
        => CreateChecked(resolution, load, _defaultSeriesSourceOptions);

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options
    )
        where T : IComparable<T>, IComparable<Instant>
    {
        var cache = new UncheckedSeriesSourceCache<T>(resolution);
        var logger = _loggerFactory.Get<LoadingSeriesSource<T>>();

        return new LoadingSeriesSource<T>(cache, resolution, load, options, logger);
    }

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options
    )
        where T : ITimeSeries, IComparable<T>
    {
        var cache = new CheckedSeriesSourceCache<T>(resolution);
        var logger = _loggerFactory.Get<LoadingSeriesSource<T>>();

        return new LoadingSeriesSource<T>(cache, resolution, load, options, logger);
    }

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>
    {
        var cache = new UncheckedSeriesSourceCache<TD>(source.Resolution);
        var logger = _loggerFactory.Get<DependentSeriesSource<TS, TD>>();

        return new DependentSeriesSource<TS, TD>(source, cache, getValues, logger);
    }

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>
    {
        var cache = new CheckedSeriesSourceCache<TD>(source.Resolution);
        var logger = _loggerFactory.Get<DependentSeriesSource<TS, TD>>();

        return new DependentSeriesSource<TS, TD>(source, cache, getValues, logger);
    }
}