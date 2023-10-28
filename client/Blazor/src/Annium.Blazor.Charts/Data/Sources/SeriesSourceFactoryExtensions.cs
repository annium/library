using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public static class SeriesSourceFactoryExtensions
{
    #region unchecked loading

    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Duration, Instant, Instant, IReadOnlyList<T>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    public static ISeriesSource<T> CreateUnchecked<T>(
        this ISeriesSourceFactory factory,
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions? options = null
    )
        where T : ITimeSeries => factory.CreateUnchecked(resolution, load, Compare, Compare, options);

    #endregion

    #region unchecked dependent

    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<TS, TD?> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<TS, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    public static ISeriesSource<TD> CreateUnchecked<TS, TD>(
        this ISeriesSourceFactory factory,
        ISeriesSource<TS> source,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues
    )
        where TD : ITimeSeries => factory.CreateUnchecked(source, getValues, Compare, Compare);

    #endregion

    #region compare

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Compare<T>(T a, T b)
        where T : ITimeSeries => a.Moment.CompareTo(b.Moment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Compare<T>(T a, Instant m)
        where T : ITimeSeries => a.Moment.CompareTo(m);

    #endregion
}
