using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class SeriesSourceFactory : ISeriesSourceFactory
{
    private readonly IServiceProvider _sp;
    private readonly ILoggerFactory _loggerFactory;

    public SeriesSourceFactory(
        IServiceProvider sp,
        ILoggerFactory loggerFactory
    )
    {
        _sp = sp;
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
        var timeProvider = _sp.Resolve<ITimeProvider>();
        var boundary = _sp.Resolve<Boundary>();
        var logger = _loggerFactory.Get<LoadingSeriesSource<TData>>();

        return new LoadingSeriesSource<TData>(timeProvider, resolution, load, options, logger);
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