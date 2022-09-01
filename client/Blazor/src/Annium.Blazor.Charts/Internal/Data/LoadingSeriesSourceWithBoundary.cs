using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Data;

internal class LoadingSeriesSourceWithBoundary<TData> : ISeriesSource<TData>, ILogSubject<LoadingSeriesSourceWithBoundary<TData>>
    where TData : ITimeSeries
{
    public Duration Resolution { get; private set; }
    public bool IsLoading => Volatile.Read(ref _isLoading) == 1;
    public ValueRange<Instant> Bounds => _boundary.Bounds;
    public ILogger<LoadingSeriesSourceWithBoundary<TData>> Logger { get; }

    private readonly List<TData> _cache = new();
    private Instant Start => _cache[0].Moment;
    private Instant End => _cache[^1].Moment;

    private readonly Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> _load;
    private readonly SeriesSourceOptions _options;
    private readonly Boundary _boundary;
    private int _isLoading;
    private int _isDisposed;

    public LoadingSeriesSourceWithBoundary(
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<TData>>> load,
        SeriesSourceOptions options,
        Boundary boundary,
        ILogger<LoadingSeriesSourceWithBoundary<TData>> logger
    )
    {
        Resolution = resolution;
        Logger = logger;
        _load = load;
        _options = options;
        _boundary = boundary;
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TData> data)
    {
        data = Array.Empty<TData>();

        var (min, max) = _boundary.GetBounds(start, end, _options.BufferZone);

        var from = Instant.Max(start, min);
        var to = Instant.Min(end, max);

        this.Log().Trace($"in {S(start)} - {S(end)}, with bounds {S(min)} - {S(max)} -> {S(from)} - {S(to)}");

        if (_cache.Count == 0)
            return GetDataFromEmptyCache(from, to);

        SyncCache(start, end);

        this.Log().Trace($"cache after sync: {S(Start)} - {S(Bounds.End)}");

        if (min < Start || End < max)
        {
            this.Log().Trace($"not enough buffered data with bounds: {S(min)} - {S(max)}");

            return false;
        }

        from = Instant.Max(start, Start);
        to = Instant.Min(end, End);

        if (from < to)
            data = GetDataFromCache(from, to);
        else
            this.Log().Trace($"non-overlapping range between: range {S(start)} - {S(end)} and cache {S(Start)} - {S(End)}");

        return true;
    }

    public TData? GetItem(Instant moment)
    {
        if (_cache.Count == 0)
            return default;

        if (moment < Start || moment > End)
            return default;

        var index = (moment.Minus(Start) / Resolution).RoundInt32();
        var item = _cache[index];

        if (item.Moment != moment)
            throw new InvalidOperationException($"Item {item} doesn't match moment {moment}");

        return item;
    }

    public void LoadItems(Instant start, Instant end, Action onLoaded)
    {
        LoadData(start, end).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    onLoaded();
                else if (t.Exception is not null)
                    throw t.Exception;
                else
                    this.Log().Error($"load done in {t.Status} status");
            }
        );
    }

    public void SetResolution(Duration resolution)
    {
        if (resolution == Resolution)
            return;

        Resolution = resolution;
        Clear();
    }

    public void Clear()
    {
        _cache.Clear();
        _boundary.Reset();
    }

    private bool GetDataFromEmptyCache(Instant from, Instant to)
    {
        // empty range is default, no data was attempted to load
        if (_boundary.HasDefaultState)
        {
            this.Log().Trace("init");
            return false;
        }

        // if range is within empty range - assume data is present
        return _boundary.Contains(from, to);
    }

    private IReadOnlyList<TData> GetDataFromCache(Instant from, Instant to)
    {
        this.Log().Trace($"get range: {S(from)} - {S(to)}");

        var startIndex = _cache.FindIndex(x => x.Moment == from);
        if (startIndex < 0)
            throw new InvalidOperationException($"Item at {S(from)} not found in cache");

        for (var i = startIndex + 1; i < _cache.Count; i++)
            if (_cache[i].Moment == to)
            {
                this.Log().Trace($"GetRange({startIndex}, {i - startIndex + 1})");

                return _cache.GetRange(startIndex, i - startIndex + 1);
            }

        throw new InvalidOperationException($"Item at {S(to)} not found in cache");
    }

    private async Task LoadData(Instant start, Instant end)
    {
        BeginLoad();

        var (min, max) = _boundary.GetBounds(start, end, _options.LoadZone);

        if (_cache.Count == 0)
        {
            this.Log().Trace($"empty cache, resolve range from min/max: {S(min)} - {S(max)} and empty range");
            var ranges = _boundary.GetUnprocessedRanges(ValueRange.Create(min, max));
            this.Log().Trace($"empty cache, load in: {ranges.Select(x => $"{S(x.Start)} - {S(x.End)}").Join("; ")}");
            foreach (var range in ranges)
            {
                if (range.Start == min)
                    _cache.InsertRange(0, await LoadInRange(range.Start, range.End));
                else
                    _cache.AddRange(await LoadInRange(range.Start, range.End));
            }
        }
        else
        {
            var from = Start - Resolution;
            var to = End + Resolution;

            this.Log().Trace($"filled cache, bounds: {S(min)} - {S(max)}, cache {S(Start)} - {S(End)}");

            if (min < from)
                _cache.InsertRange(0, await LoadInRange(min, from));

            if (to < max)
                _cache.AddRange(await LoadInRange(to, max));
        }

        AdjustChartBounds(min, max);

        EndLoad();
    }

    private async Task<IReadOnlyList<TData>> LoadInRange(Instant start, Instant end)
    {
        this.Log().Trace($"{S(start)} - {S(end)}");

        var items = await _load(Resolution, start, end);

        this.Log().Trace(items.Count > 0 ? $"loaded {items.Count} items in {S(items[0].Moment)} - {S(items[^1].Moment)}" : "no items loaded");

        return items;
    }

    private void AdjustChartBounds(Instant start, Instant end)
    {
        if (_cache.Count == 0)
            _boundary.ShrinkBounds(start, end);
        else
            _boundary.ExtendBounds(start, end);
    }

    private void SyncCache(Instant start, Instant end)
    {
        var (min, max) = _boundary.GetBounds(start, end, _options.CacheZone);
        this.Log().Trace($"set {S(min)} - {S(max)} for {S(Start)} - {S(End)}");

        var index = _cache.FindIndex(x => x.Moment == min);
        if (index > 0)
        {
            this.Log().Trace($"MM: {S(Bounds.Start)} - {S(Bounds.End)}. Cache: {S(Start)} - {S(End)}. Bounds {S(min)} - {S(max)}. Remove range (0, {index}) from {_cache.Count} items");
            _cache.RemoveRange(0, index);
        }

        index = _cache.FindLastIndex(x => x.Moment == max);
        if (index > 0 && index < _cache.Count - 1)
        {
            this.Log().Trace($"MM: {S(Bounds.Start)} - {S(Bounds.End)}. Cache: {S(Start)} - {S(End)}. Bounds {S(min)} - {S(max)}. Remove range ({index}, {_cache.Count - index}) from {_cache.Count} items");
            _cache.RemoveRange(index, _cache.Count - index);
        }

        ValidateCacheIntegrity();
    }

    private void ValidateCacheIntegrity()
    {
        if (_cache.Count <= 1)
            return;

        for (var i = 1; i < _cache.Count; i++)
        {
            var diff = _cache[i].Moment - _cache[i - 1].Moment;
            if (diff != Resolution)
                throw new InvalidOperationException($"Cache integrity failure: {_cache[i - 1]}, {_cache[i]}. Diff: {diff}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BeginLoad() => Volatile.Write(ref _isLoading, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndLoad() => Volatile.Write(ref _isLoading, 0);

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            throw new InvalidOperationException($"{GetType().FriendlyName()} is already disposed");

        _cache.Clear();
    }
}