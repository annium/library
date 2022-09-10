using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    #region unchecked loading

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>
        => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution), (_, start, end) => Task.FromResult(load(start, end)), options);

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>
        => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution), (duration, start, end) => Task.FromResult(load(duration, start, end)), options);

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>
        => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution), (_, start, end) => load(start, end), options);

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>
        => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution), load, options);

    #endregion

    #region checked loading

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>
        => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), (_, start, end) => Task.FromResult(load(start, end)), options);

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>
        => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), (duration, start, end) => Task.FromResult(load(duration, start, end)), options);

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>
        => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), (_, start, end) => load(start, end), options);

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>
        => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), load, options);

    #endregion

    #region unchecked dependent

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>
        => Create(source, new UncheckedSeriesSourceCache<TD>(source.Resolution), (chunk, _, _, _) => chunk.Select(getValues).ToArray());

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>
        => Create(source, new UncheckedSeriesSourceCache<TD>(source.Resolution), (chunk, resolution, start, end) => chunk.SelectMany(x => getValues(x, resolution, start, end)).ToArray());

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>
        => Create(source, new UncheckedSeriesSourceCache<TD>(source.Resolution), getValues);

    #endregion

    #region checked dependent

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues
    )
        where TD : ITimeSeries, IComparable<TD>
        => Create(source, new CheckedSeriesSourceCache<TD>(source.Resolution), (chunk, _, _, _) => chunk.Select(getValues).ToArray());

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>
        => Create(source, new CheckedSeriesSourceCache<TD>(source.Resolution), (chunk, resolution, start, end) => chunk.SelectMany(x => getValues(x, resolution, start, end)).ToArray());

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>
        => Create(source, new CheckedSeriesSourceCache<TD>(source.Resolution), getValues);

    #endregion

    #region base

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ISeriesSource<T> Create<T>(
        Duration resolution,
        ISeriesSourceCache<T> cache,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options
    )
        where T : IComparable<T>
    {
        var logger = _loggerFactory.Get<LoadingSeriesSource<T>>();

        return new LoadingSeriesSource<T>(cache, resolution, load, options ?? _defaultSeriesSourceOptions, logger);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ISeriesSource<TD> Create<TS, TD>(
        ISeriesSource<TS> source,
        ISeriesSourceCache<TD> cache,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : IComparable<TD>
    {
        var logger = _loggerFactory.Get<DependentSeriesSource<TS, TD>>();

        return new DependentSeriesSource<TS, TD>(source, cache, getValues, logger);
    }

    #endregion

    // public ISeriesSource<T> CreateChecked<T>(
    //     Duration resolution,
    //     Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
    //     SeriesSourceOptions options
    // )
    //     where T : ITimeSeries, IComparable<T>
    // {
    //     var cache = new CheckedSeriesSourceCache<T>(resolution);
    //     var logger = _loggerFactory.Get<LoadingSeriesSource<T>>();
    //
    //     return new LoadingSeriesSource<T>(cache, resolution, load, options, logger);
    // }
    //
    // private ISeriesSource<TD> CreateUnchecked<TS, TD>(
    //     ISeriesSource<TS> source,
    //     Func<IReadOnlyList<TS>, Instant, Instant, IReadOnlyCollection<TD>> getValues
    // )
    //     where TD : IComparable<TD>, IComparable<Instant>
    // {
    //     var cache = new UncheckedSeriesSourceCache<TD>(source.Resolution);
    //     var logger = _loggerFactory.Get<DependentSeriesSource<TS, TD>>();
    //
    //     return new DependentSeriesSource<TS, TD>(source, cache, getValues, logger);
    // }
}