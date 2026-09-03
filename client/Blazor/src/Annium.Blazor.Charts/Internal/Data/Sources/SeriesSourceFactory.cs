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

/// <summary>
/// Factory for creating various types of series sources with different loading and caching strategies
/// </summary>
internal class SeriesSourceFactory : ISeriesSourceFactory
{
    /// <summary>
    /// Logger instance for the factory
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the SeriesSourceFactory class
    /// </summary>
    /// <param name="logger">Logger instance for the factory</param>
    public SeriesSourceFactory(ILogger logger)
    {
        _logger = logger;
    }

    #region unchecked loading

    /// <summary>
    /// Creates an unchecked series source with synchronous loading
    /// </summary>
    /// <typeparam name="T">The type of data items in the series</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data synchronously</param>
    /// <param name="compare">Function to compare two data items</param>
    /// <param name="compareToMoment">Function to compare a data item to a moment in time</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
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

    /// <summary>
    /// Creates an unchecked series source with synchronous loading that includes resolution parameter
    /// </summary>
    /// <typeparam name="T">The type of data items in the series</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data synchronously with resolution parameter</param>
    /// <param name="compare">Function to compare two data items</param>
    /// <param name="compareToMoment">Function to compare a data item to a moment in time</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
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

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading
    /// </summary>
    /// <typeparam name="T">The type of data items in the series</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data asynchronously</param>
    /// <param name="compare">Function to compare two data items</param>
    /// <param name="compareToMoment">Function to compare a data item to a moment in time</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
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

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading that includes resolution parameter
    /// </summary>
    /// <typeparam name="T">The type of data items in the series</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data asynchronously with resolution parameter</param>
    /// <param name="compare">Function to compare two data items</param>
    /// <param name="compareToMoment">Function to compare a data item to a moment in time</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
    public ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    ) => Create(resolution, new UncheckedSeriesSourceCache<T>(resolution, compare, compareToMoment), load, options);

    #endregion

    #region checked loading

    /// <summary>
    /// Creates a checked series source with synchronous loading for time series data
    /// </summary>
    /// <typeparam name="T">The type of time series data items</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data synchronously</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
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

    /// <summary>
    /// Creates a checked series source with synchronous loading that includes resolution parameter for time series data
    /// </summary>
    /// <typeparam name="T">The type of time series data items</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data synchronously with resolution parameter</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
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

    /// <summary>
    /// Creates a checked series source with asynchronous loading for time series data
    /// </summary>
    /// <typeparam name="T">The type of time series data items</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data asynchronously</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries =>
        Create(resolution, new CheckedSeriesSourceCache<T>(resolution), (_, start, end) => load(start, end), options);

    /// <summary>
    /// Creates a checked series source with asynchronous loading that includes resolution parameter for time series data
    /// </summary>
    /// <typeparam name="T">The type of time series data items</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="load">Function to load data asynchronously with resolution parameter</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new series source instance</returns>
    public ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => Create(resolution, new CheckedSeriesSourceCache<T>(resolution), load, options);

    #endregion

    #region unchecked dependent

    /// <summary>
    /// Creates an unchecked dependent series source that transforms individual source items
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a source item to a destination item</param>
    /// <param name="compare">Function to compare two destination items</param>
    /// <param name="compareToMoment">Function to compare a destination item to a moment in time</param>
    /// <returns>A new dependent series source instance</returns>
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

    /// <summary>
    /// Creates an unchecked dependent series source that transforms source items with time context
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a source item to multiple destination items with time context</param>
    /// <param name="compare">Function to compare two destination items</param>
    /// <param name="compareToMoment">Function to compare a destination item to a moment in time</param>
    /// <returns>A new dependent series source instance</returns>
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

    /// <summary>
    /// Creates an unchecked dependent series source that transforms collections of source items
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a collection of source items to destination items</param>
    /// <param name="compare">Function to compare two destination items</param>
    /// <param name="compareToMoment">Function to compare a destination item to a moment in time</param>
    /// <returns>A new dependent series source instance</returns>
    public ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    ) => Create(source, new UncheckedSeriesSourceCache<TD>(source.Resolution, compare, compareToMoment), getValues);

    #endregion

    #region checked dependent

    /// <summary>
    /// Creates a checked dependent series source that transforms individual source items for time series data
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination time series data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a source item to a destination item</param>
    /// <returns>A new dependent series source instance</returns>
    public ISeriesSource<TD> CreateChecked<TS, TD>(ISeriesSource<TS> source, Func<TS, TD> getValues)
        where TD : ITimeSeries =>
        Create(
            source,
            new CheckedSeriesSourceCache<TD>(source.Resolution),
            (chunk, _, _, _) => chunk.Select(getValues).ToArray()
        );

    /// <summary>
    /// Creates a checked dependent series source that transforms source items with time context for time series data
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination time series data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a source item to multiple destination items with time context</param>
    /// <returns>A new dependent series source instance</returns>
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

    /// <summary>
    /// Creates a checked dependent series source that transforms collections of source items for time series data
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination time series data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="getValues">Function to transform a collection of source items to destination items</param>
    /// <returns>A new dependent series source instance</returns>
    public ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => Create(source, new CheckedSeriesSourceCache<TD>(source.Resolution), getValues);

    #endregion

    #region base

    /// <summary>
    /// Creates a loading series source with the specified cache and load function
    /// </summary>
    /// <typeparam name="T">The type of data items in the series</typeparam>
    /// <param name="resolution">The time resolution for the series</param>
    /// <param name="cache">The cache for storing loaded data</param>
    /// <param name="load">Function to load data asynchronously</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A new loading series source instance</returns>
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

    /// <summary>
    /// Creates a dependent series source with the specified source, cache, and transformation function
    /// </summary>
    /// <typeparam name="TS">The type of source data items</typeparam>
    /// <typeparam name="TD">The type of destination data items</typeparam>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="cache">The cache for storing transformed data</param>
    /// <param name="getValues">Function to transform source data into destination data</param>
    /// <returns>A new dependent series source instance</returns>
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
