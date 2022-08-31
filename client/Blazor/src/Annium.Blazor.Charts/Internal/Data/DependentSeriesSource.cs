using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class DependentSeriesSource<TSource, TData> :
    ISeriesSource<TData>,
    ILogSubject<DependentSeriesSource<TSource, TData>>
    where TSource : ITimeSeries
    where TData : ITimeSeries
{
    public ILogger<DependentSeriesSource<TSource, TData>> Logger { get; }
    public Duration Resolution => _source.Resolution;
    public bool IsLoading => _source.IsLoading;
    public ValueRange<Instant> Bounds => _source.Bounds;
    private readonly ISeriesSource<TSource> _source;
    private readonly Func<TSource, TData> _getValue;

    public DependentSeriesSource(
        ISeriesSource<TSource> source,
        Func<TSource, TData> getValue,
        ILogger<DependentSeriesSource<TSource, TData>> logger
    )
    {
        Logger = logger;
        _source = source;
        _getValue = getValue;
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TData> data)
    {
        // this.Log().Trace($"get data in {start} - {end}");
        var result = _source.GetItems(start, end, out var sourceData);
        data = sourceData.Select(_getValue).ToArray();

        return result;
    }

    public TData? GetItem(Instant moment)
    {
        var item = _source.GetItem(moment);

        return item is null ? default : _getValue(item);
    }

    public void LoadItems(Instant start, Instant end, Action onLoaded)
    {
        // this.Log().Trace($"get data in {start} - {end}");
        _source.LoadItems(start, end, onLoaded);
    }

    public void SetResolution(Duration resolution) => _source.SetResolution(resolution);

    public void Clear() => _source.Clear();

    public void Dispose()
    {
    }
}