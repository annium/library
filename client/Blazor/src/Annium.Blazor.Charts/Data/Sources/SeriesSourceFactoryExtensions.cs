using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

/// <summary>
/// Extension methods for ISeriesSourceFactory to simplify series source creation for ITimeSeries types.
/// </summary>
public static class SeriesSourceFactoryExtensions
{
    #region unchecked loading

    /// <summary>
    /// Creates an unchecked series source with synchronous loading function for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    /// <summary>
    /// Creates an unchecked series source with synchronous loading function that includes resolution for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range with resolution parameter.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading function for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading function that includes resolution for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range with resolution parameter.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    #endregion

    #region unchecked dependent

    /// <summary>
    /// Creates an unchecked dependent series source that transforms data from another series source for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items.</param>
    /// <returns>A new dependent series source instance.</returns>
    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<TS, TD?> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    /// <summary>
    /// Creates an unchecked dependent series source that transforms data from another series source with resolution and time range parameters for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items with resolution and time range parameters.</param>
    /// <returns>A new dependent series source instance.</returns>
    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    /// <summary>
    /// Creates an unchecked dependent series source that transforms a collection of data from another series source for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="factory">The series source factory.</param>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform a collection of source items to destination items with resolution and time range parameters.</param>
    /// <returns>A new dependent series source instance.</returns>
    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    #endregion

    #region compare

    /// <summary>
    /// Compares two ITimeSeries items by their moment in time.
    /// </summary>
    /// <typeparam name="T">The type that implements ITimeSeries.</typeparam>
    /// <param name="a">The first item to compare.</param>
    /// <param name="b">The second item to compare.</param>
    /// <returns>A value indicating the relative order of the items.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Compare<T>(T a, T b)
        where T : ITimeSeries => a.Moment.CompareTo(b.Moment);

    /// <summary>
    /// Compares an ITimeSeries item to a specific moment in time.
    /// </summary>
    /// <typeparam name="T">The type that implements ITimeSeries.</typeparam>
    /// <param name="a">The item to compare.</param>
    /// <param name="m">The moment to compare against.</param>
    /// <returns>A value indicating the relative order of the item and moment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Compare<T>(T a, Instant m)
        where T : ITimeSeries => a.Moment.CompareTo(m);

    #endregion
}
