using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSourceFactory
{
    #region unchecked loading

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>;

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>;

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>;

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : IComparable<T>, IComparable<Instant>;

    #endregion

    #region checked loading

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions? options = null
    )
        where T : ITimeSeries, IComparable<T>;

    #endregion

    #region unchecked dependent

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>;

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>;

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>;

    #endregion

    #region checked dependent

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, TD> getValues
    )
        where TD : ITimeSeries, IComparable<TD>;

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>;

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>;

    #endregion
}