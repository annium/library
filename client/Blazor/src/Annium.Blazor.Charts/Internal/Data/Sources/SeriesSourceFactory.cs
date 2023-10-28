using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Logging;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class SeriesSourceFactory : ISeriesSourceFactory
{
    private readonly ILogger _logger;

    public SeriesSourceFactory(ILogger logger)
    {
        _logger = logger;
    }

    #region unchecked loading

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    ) =>
        Create(
            resolution,
            new UncheckedSeriesSourceCache<T>(resolution, compare, compareToMoment),
            (_, start, end) => Task.FromResult(load(start, end)),
            options
        );

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    ) =>
        Create(
            resolution,
            new UncheckedSeriesSourceCache<T>(resolution, compare, compareToMoment),
            (duration, start, end) => Task.FromResult(load(duration, start, end)),
            options
        );

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    ) =>
        Create(
            resolution,
            new UncheckedSeriesSourceCache<T>(resolution, compare, compareToMoment),
            (_, start, end) => load(start, end),
            options
        );

    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    ) => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution, compare, compareToMoment), load, options);

    #endregion

    #region checked loading

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries =>
        Create(
            resolution,
            new CheckedSeriesSourceCache<T>(resolution),
            (_, start, end) => Task.FromResult(load(start, end)),
            options
        );

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries =>
        Create(
            resolution,
            new CheckedSeriesSourceCache<T>(resolution),
            (duration, start, end) => Task.FromResult(load(duration, start, end)),
            options
        );

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries =>
        Create(resolution, new CheckedSeriesSourceCache<T>(resolution), (_, start, end) => load(start, end), options);

    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), load, options);

    #endregion

    #region unchecked dependent

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD?> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    ) =>
        Create(
            source,
            new UncheckedSeriesSourceCache<TD>(source.Resolution, compare, compareToMoment),
            (chunk, _, _, _) =>
            {
                var result = new List<TD>(chunk.Count);
                foreach (var sourceItem in chunk)
                {
                    var resultItem = getValues(sourceItem);
                    if (resultItem is not null)
                        result.Add(resultItem);
                }

                return result;
            }
        );

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    ) =>
        Create(
            source,
            new UncheckedSeriesSourceCache<TD>(source.Resolution, compare, compareToMoment),
            (chunk, resolution, start, end) => chunk.SelectMany(x => getValues(x, resolution, start, end)).ToArray()
        );

    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    ) => Create(source, new UncheckedSeriesSourceCache<TD>(source.Resolution, compare, compareToMoment), getValues);

    #endregion

    #region checked dependent

    public ISeriesSource<TD> CreateChecked<TS, TD>(ISeriesSource<TS> source, Func<TS, TD> getValues)
        where TD : ITimeSeries =>
        Create(
            source,
            new CheckedSeriesSourceCache<TD>(source.Resolution),
            (chunk, _, _, _) => chunk.Select(getValues).ToArray()
        );

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries =>
        Create(
            source,
            new CheckedSeriesSourceCache<TD>(source.Resolution),
            (chunk, resolution, start, end) => chunk.SelectMany(x => getValues(x, resolution, start, end)).ToArray()
        );

    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => Create(source, new CheckedSeriesSourceCache<TD>(source.Resolution), getValues);

    #endregion

    #region base

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ISeriesSource<T> Create<T>(
        Duration resolution,
        ISeriesSourceCache<T> cache,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options
    )
    {
        return new LoadingSeriesSource<T>(
            cache,
            resolution,
            load,
            options ?? SeriesSourceOptionsBuilder.Default,
            _logger
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ISeriesSource<TD> Create<TS, TD>(
        ISeriesSource<TS> source,
        ISeriesSourceCache<TD> cache,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
    {
        return new DependentSeriesSource<TS, TD>(source, cache, getValues, _logger);
    }

    #endregion
}
