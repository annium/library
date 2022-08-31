using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class SeriesSourceFactory : ISeriesSourceFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILoggerFactory _loggerFactory;

    public SeriesSourceFactory(
        ITimeProvider timeProvider,
        ILoggerFactory loggerFactory
    )
    {
        _timeProvider = timeProvider;
        _loggerFactory = loggerFactory;
    }

    public ISeriesSource<TData> Create<TData>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> load
    )
        where TData : ITimeSeries
        => Create(resolution, load, new SeriesSourceOptions(1L, 3L, 8L));

    public ISeriesSource<TData> Create<TData>(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> load,
        SeriesSourceOptions options
    )
        where TData : ITimeSeries
    {
        return new LoadingSeriesSource<TData>(_timeProvider, resolution, load, options, _loggerFactory.Get<LoadingSeriesSource<TData>>());
    }

    public ISeriesSource<TData> Create<TSource, TData>(
        ISeriesSource<TSource> source,
        Func<TSource, TData> getValue
    )
        where TSource : ITimeSeries
        where TData : ITimeSeries
    {
        return new DependentSeriesSource<TSource, TData>(source, getValue, _loggerFactory.Get<DependentSeriesSource<TSource, TData>>());
    }
}