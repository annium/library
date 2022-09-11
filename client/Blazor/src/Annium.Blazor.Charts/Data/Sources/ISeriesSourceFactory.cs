using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSourceFactory
{
    #region unchecked loading

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        SeriesSourceOptions? options = null
    );

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        SeriesSourceOptions? options = null
    );

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        SeriesSourceOptions? options = null
    );

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment,
        SeriesSourceOptions? options = null
    );

    #endregion

    #region checked loading

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries;

    #endregion

    #region unchecked dependent

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        Func<TD, TD, int> compare,
        Func<TD, Instant, int> compareToMoment
    );

    #endregion

    #region checked dependent

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues
    )
        where TD : ITimeSeries;

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries;

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries;

    #endregion
}