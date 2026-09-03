using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

/// <summary>
/// Factory for creating series sources with various loading strategies and data types.
/// </summary>
public interface ISeriesSourceFactory
{
    #region unchecked loading

    /// <summary>
    /// Creates an unchecked series source with synchronous loading function.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range.</param>
    /// <param name="compare">The function to compare two items.</param>
    /// <param name="compareToMoment">The function to compare an item to a moment in time.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    );

    /// <summary>
    /// Creates an unchecked series source with synchronous loading function that includes resolution.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range with resolution parameter.</param>
    /// <param name="compare">The function to compare two items.</param>
    /// <param name="compareToMoment">The function to compare an item to a moment in time.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    );

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading function.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range.</param>
    /// <param name="compare">The function to compare two items.</param>
    /// <param name="compareToMoment">The function to compare an item to a moment in time.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    );

    /// <summary>
    /// Creates an unchecked series source with asynchronous loading function that includes resolution.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range with resolution parameter.</param>
    /// <param name="compare">The function to compare two items.</param>
    /// <param name="compareToMoment">The function to compare an item to a moment in time.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        ISeriesSourceOptions? options = null
    );

    #endregion

    #region checked loading

    /// <summary>
    /// Creates a checked series source with synchronous loading function for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    /// <summary>
    /// Creates a checked series source with synchronous loading function that includes resolution for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The function to load data for a time range with resolution parameter.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    /// <summary>
    /// Creates a checked series source with asynchronous loading function for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    /// <summary>
    /// Creates a checked series source with asynchronous loading function that includes resolution for ITimeSeries types.
    /// </summary>
    /// <typeparam name="T">The type of data items in the series that implements ITimeSeries.</typeparam>
    /// <param name="resolution">The resolution of the data series.</param>
    /// <param name="load">The asynchronous function to load data for a time range with resolution parameter.</param>
    /// <param name="options">Optional series source configuration options.</param>
    /// <returns>A new series source instance.</returns>
    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    #endregion

    #region unchecked dependent

    /// <summary>
    /// Creates an unchecked dependent series source that transforms data from another series source.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items.</param>
    /// <param name="compare">The function to compare two destination items.</param>
    /// <param name="compareToMoment">The function to compare a destination item to a moment in time.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD?> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    /// <summary>
    /// Creates an unchecked dependent series source that transforms data from another series source with resolution and time range parameters.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items with resolution and time range parameters.</param>
    /// <param name="compare">The function to compare two destination items.</param>
    /// <param name="compareToMoment">The function to compare a destination item to a moment in time.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    /// <summary>
    /// Creates an unchecked dependent series source that transforms a collection of data from another series source.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform a collection of source items to destination items with resolution and time range parameters.</param>
    /// <param name="compare">The function to compare two destination items.</param>
    /// <param name="compareToMoment">The function to compare a destination item to a moment in time.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    #endregion

    #region checked dependent

    /// <summary>
    /// Creates a checked dependent series source that transforms data from another series source for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateChecked<TS, TD>(ISeriesSource<TS> source, Func<TS, TD> getValues)
        where TD : ITimeSeries;

    /// <summary>
    /// Creates a checked dependent series source that transforms data from another series source with resolution and time range parameters for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform source items to destination items with resolution and time range parameters.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries;

    /// <summary>
    /// Creates a checked dependent series source that transforms a collection of data from another series source for ITimeSeries types.
    /// </summary>
    /// <typeparam name="TS">The type of source data items.</typeparam>
    /// <typeparam name="TD">The type of destination data items that implements ITimeSeries.</typeparam>
    /// <param name="source">The source series to depend on.</param>
    /// <param name="getValues">The function to transform a collection of source items to destination items with resolution and time range parameters.</param>
    /// <returns>A new dependent series source instance.</returns>
    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries;

    #endregion
}
