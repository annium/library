using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSourceFactory
{
    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load
    )
        where T : IComparable<T>, IComparable<Instant>;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load
    )
        where T : ITimeSeries, IComparable<T>;

    ISeriesSource<T> CreateUnchecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options
    )
        where T : IComparable<T>, IComparable<Instant>;

    ISeriesSource<T> CreateChecked<T>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options
    )
        where T : ITimeSeries, IComparable<T>;

    ISeriesSource<TD> CreateUnchecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues
    )
        where TD : IComparable<TD>, IComparable<Instant>;

    ISeriesSource<TD> CreateChecked<TS, TD>(
        ISeriesSource<TS> source,
        Func<TS, Instant, Instant, IEnumerable<TD>> getValues
    )
        where TD : ITimeSeries, IComparable<TD>;
}