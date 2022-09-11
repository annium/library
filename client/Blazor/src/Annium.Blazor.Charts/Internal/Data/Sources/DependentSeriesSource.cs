using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class DependentSeriesSource<TS, TD> :
    ISeriesSource<TD>,
    ILogSubject<DependentSeriesSource<TS, TD>>
    where TD : IComparable<TD>
{
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public ILogger<DependentSeriesSource<TS, TD>> Logger { get; }
    public Duration Resolution => _source.Resolution;
    public bool IsLoading => _source.IsLoading;
    public ValueRange<Instant> Bounds => _source.Bounds;
    private readonly ISeriesSource<TS> _source;
    private readonly ISeriesSourceCache<TD> _cache;
    private readonly Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> _getValues;

    public DependentSeriesSource(
        ISeriesSource<TS> source,
        ISeriesSourceCache<TD> cache,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        ILogger<DependentSeriesSource<TS, TD>> logger
    )
    {
        Logger = logger;
        _source = source;
        _cache = cache;
        _getValues = getValues;
        _cache.OnBoundsChange += bounds => OnBoundsChange(bounds);
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TD> data)
    {
        if (_cache.HasData(start, end))
        {
            this.Log().Trace($"get data in {start.S()} - {end.S()}: found in cache");
            data = _cache.GetData(start, end);

            return true;
        }

        if (!_source.GetItems(start, end, out _))
        {
            this.Log().Trace($"get data in {start.S()} - {end.S()}: missing in source");
            data = Array.Empty<TD>();

            return false;
        }

        this.Log().Trace($"get data in {start.S()} - {end.S()}: found in source, fill cache");

        var emptyRanges = _cache.GetEmptyRanges(start, end);
        foreach (var range in emptyRanges)
        {
            if (!_source.GetItems(range.Start, range.End, out var rangeSource))
                throw new InvalidOperationException($"Series source {_source} invalid behavior: expected to get data in range {range.S()}");

            var rangeData = _getValues(rangeSource, _source.Resolution, range.Start, range.End);
            this.Log().Trace($"save {rangeData.Count} item(s) ({rangeSource.Count} sourced) in {range.S()} to cache");
            _cache.AddData(range.Start, range.End, rangeData);
        }

        data = _cache.GetData(start, end);
        this.Log().Trace($"get data in {start} - {end}: served from cache");

        return true;
    }

    public TD? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    public void LoadItems(Instant start, Instant end, Action onLoaded)
    {
        // this.Log().Trace($"get data in {start} - {end}");
        _source.LoadItems(start, end, onLoaded);
    }

    public void SetResolution(Duration resolution)
    {
        _source.SetResolution(resolution);
        _cache.SetResolution(resolution);
    }

    public void Clear()
    {
        _source.Clear();
        _cache.Clear();
    }

    public void Dispose()
    {
        _source.Clear();
        _cache.Clear();
    }
}