using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Data.Models;
using Annium.Logging;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class DependentSeriesSource<TS, TD> : ISeriesSource<TD>, ILogSubject
{
    public event Action Loaded = delegate { };
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public ILogger Logger { get; }
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
        ILogger logger
    )
    {
        Logger = logger;
        _source = source;
        _cache = cache;
        _getValues = getValues;
        _source.Loaded += TriggerLoaded;
        _cache.OnBoundsChange += TriggerBoundsChanged;
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TD> data)
    {
        if (_cache.HasData(start, end))
        {
            this.Trace<string, string>("get data in {start} - {end}: found in cache", start.S(), end.S());
            data = _cache.GetData(start, end);

            return true;
        }

        if (!_source.GetItems(start, end, out _))
        {
            this.Trace<string, string>("get data in {start} - {end}: missing in source", start.S(), end.S());
            data = [];

            return false;
        }

        this.Trace<string, string>("get data in {start} - {end}: found in source, fill cache", start.S(), end.S());

        var emptyRanges = _cache.GetEmptyRanges(start, end);
        foreach (var range in emptyRanges)
        {
            if (!_source.GetItems(range.Start, range.End, out var rangeSource))
                throw new InvalidOperationException(
                    $"Series source {_source} invalid behavior: expected to get data in range {range.S()}"
                );

            var rangeData = _getValues(rangeSource, _source.Resolution, range.Start, range.End);
            this.Trace<int, int, string>(
                "save {rangeDataCount} item(s) ({rangeSourceCount} sourced) in {range} to cache",
                rangeData.Count,
                rangeSource.Count,
                range.S()
            );
            _cache.AddData(range.Start, range.End, rangeData);
        }

        data = _cache.GetData(start, end);
        this.Trace<string, string>("get data in {start} - {end}: served from cache", start.S(), end.S());

        return true;
    }

    public TD? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    public void LoadItems(Instant start, Instant end)
    {
        // this.Trace($"get data in {start} - {end}");
        _source.LoadItems(start, end);
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

    private void TriggerLoaded() => Loaded();

    private void TriggerBoundsChanged(ValueRange<Instant> bounds) => OnBoundsChange(bounds);

    public void Dispose()
    {
        _source.Clear();
        _cache.Clear();
        _source.Loaded -= TriggerLoaded;
        _cache.OnBoundsChange -= TriggerBoundsChanged;
    }
}
