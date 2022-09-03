using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data;

internal class DependentSeriesSource<TS, TD> :
    ISeriesSource<TD>,
    ILogSubject<DependentSeriesSource<TS, TD>>
    where TS : ITimeSeries
    where TD : ITimeSeries
{
    public ILogger<DependentSeriesSource<TS, TD>> Logger { get; }
    public Duration Resolution => _source.Resolution;
    public bool IsLoading => _source.IsLoading;
    public ValueRange<Instant> Bounds => _source.Bounds;
    private readonly ISeriesSource<TS> _source;
    private readonly Func<TS, TD?> _getValue;

    public DependentSeriesSource(
        ISeriesSource<TS> source,
        Func<TS, TD?> getValue,
        ILogger<DependentSeriesSource<TS, TD>> logger
    )
    {
        Logger = logger;
        _source = source;
        _getValue = getValue;
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TD> data)
    {
        // this.Log().Trace($"get data in {start} - {end}");
        var result = _source.GetItems(start, end, out var sourceData);
        data = sourceData.Select(_getValue).OfType<TD>().ToArray();

        return result;
    }

    public TD? GetItem(Instant moment)
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