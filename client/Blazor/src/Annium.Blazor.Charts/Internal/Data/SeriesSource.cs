using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using Annium.NodaTime.Extensions;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Internal.Data;

internal class SeriesSource<T> : ISeriesSource<T>, ILogSubject
    where T : ITimeSeries
{
    public bool IsLoading => Volatile.Read(ref _isLoading) == 1;
    public ILogger Logger { get; }
    private readonly Duration _minute = Duration.FromMinutes(1);
    private const long BufferZone = 1L;
    private const long LoadZone = 3L;
    private const long CacheZone = 8L;

    public ValueRange<Instant> Bounds { get; }
    private readonly ManagedValueRange<Instant> _emptyBefore;
    private readonly ManagedValueRange<Instant> _emptyRange;
    private readonly ManagedValueRange<Instant> _emptyAfter;

    private readonly List<T> _cache = new();
    private Instant Start => _cache[0].Moment;
    private Instant End => _cache[^1].Moment;

    private Func<Instant, Instant, Task<IReadOnlyList<T>>> _load = default!;
    private int _isLoading;
    private int _isDisposed;

    public SeriesSource(
        ITimeProvider timeProvider,
        ILogger<SeriesSource<T>> logger
    )
    {
        Logger = logger;
        var now = timeProvider.Now.FloorToMinute();
        _emptyBefore = ValueRange.Create(Instant.MinValue, now - Duration.FromDays(10000));
        _emptyRange = ValueRange.Create(now, now);
        _emptyAfter = ValueRange.Create(now, Instant.MaxValue);
        Bounds = ValueRange.Create(() => _emptyBefore.End, () => _emptyAfter.Start);
    }

    public void Init(
        Func<Instant, Instant, Task<IReadOnlyList<T>>> load
    )
    {
        _load = load;
    }

    public bool GetData(Instant start, Instant end, out IReadOnlyList<T> data)
    {
        data = Array.Empty<T>();

        var (min, max) = GetBounds(start, end, BufferZone);

        var from = Instant.Max(start, min);
        var to = Instant.Min(end, max);

        // this.Log().Trace($"in {S(start)} - {S(end)}, with bounds {S(min)} - {S(max)} -> {S(from)} - {S(to)}");

        if (_cache.Count == 0)
            return GetDataFromEmptyCache(from, to);

        SyncCache(start, end);

        // this.Log().Trace($"cache after sync: {S(Start)} - {S(Max)}");

        if (min < Start || End < max)
        {
            // this.Log().Trace($"not enough buffered data with bounds: {S(min)} - {S(max)}");

            return false;
        }

        from = Instant.Max(start, Start);
        to = Instant.Min(end, End);

        if (from < to)
            data = GetDataFromCache(from, to);
        // else
        //     this.Log().Trace($"non-overlapping range between: range {S(start)} - {S(end)} and cache {S(Start)} - {S(End)}");

        return true;
    }

    public void LoadData(Instant start, Instant end, Action onLoaded)
    {
        LoadData(start, end).ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
                onLoaded();
            else if (t.Exception is not null)
                throw t.Exception;
            else
                this.Log().Error($"load done in {t.Status} status");
        });
    }

    private bool GetDataFromEmptyCache(Instant from, Instant to)
    {
        // empty range is default, no data was attempted to load
        if (_emptyRange.Start == _emptyRange.End)
        {
            // this.Log().Trace("init");
            return false;
        }

        // if range is withing empty range - assume data is present
        if (_emptyRange.Contains(from, RangeBounds.Both) && _emptyRange.Contains(to, RangeBounds.Both))
        {
            // this.Log().Trace($"in empty range {S(_emptyRange.Start)} - {S(_emptyRange.End)}");
            return true;
        }

        // this.Log().Trace($"out empty range {S(_emptyRange.Start)} - {S(_emptyRange.End)}");

        return false;
    }

    private IReadOnlyList<T> GetDataFromCache(Instant from, Instant to)
    {
        // this.Log().Trace($"get range: {S(from)} - {S(to)}");

        var startIndex = _cache.FindIndex(x => x.Moment == from);
        if (startIndex < 0)
            throw new InvalidOperationException($"Item at {S(from)} not found in cache");

        for (var i = startIndex + 1; i < _cache.Count; i++)
            if (_cache[i].Moment == to)
            {
                // this.Log().Trace($"GetRange({startIndex}, {i - startIndex + 1})");

                return _cache.GetRange(startIndex, i - startIndex + 1);
            }

        throw new InvalidOperationException($"Item at {S(to)} not found in cache");
    }

    private async Task LoadData(Instant start, Instant end)
    {
        BeginLoad();

        var (min, max) = GetBounds(start, end, LoadZone);

        if (_cache.Count == 0)
        {
            var ranges = ValueRange.Create(min, max) - _emptyRange;
            // this.Log().Trace($"empty cache, load in: {ranges.Select(x => $"{S(x.Start)} - {S(x.End)}").Join("; ")}");
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
            var from = Start - _minute;
            var to = End + _minute;

            // this.Log().Trace($"filled cache, bounds: {S(min)} - {S(max)}, cache {S(Start)} - {S(End)}");

            if (min < from)
                _cache.InsertRange(0, await LoadInRange(min, from));

            if (to < max)
                _cache.AddRange(await LoadInRange(to, max));
        }

        AdjustChartBounds(min, max);

        EndLoad();
    }

    private async Task<IReadOnlyList<T>> LoadInRange(Instant start, Instant end)
    {
        // this.Log().Trace($"{S(start)} - {S(end)}");

        var items = await _load(start, end);

        // this.Log().Trace(items.Count > 0 ? $"loaded {items.Count} items in {S(items[0].Moment)} - {S(items[^1].Moment)}" : "no items loaded");

        return items;
    }

    private void AdjustChartBounds(Instant start, Instant end)
    {
        if (_cache.Count == 0)
        {
            var emptyStart = Instant.Min(_emptyRange.Start, start);
            var emptyEnd = Instant.Max(_emptyRange.End, end);
            // if (emptyStart != _emptyRange.Start)
            //     this.Log().Trace($"empty cache, update emptyRange.Start: {S(_emptyRange.Start)} -> {S(emptyStart)}");
            // if (emptyEnd != _emptyRange.End)
            //     this.Log().Trace($"empty cache, update emptyRange.End: {S(_emptyRange.End)} -> {S(emptyEnd)}");
            _emptyRange.SetStart(emptyStart);
            _emptyRange.SetEnd(emptyEnd);
        }
        else
        {
            if (Start > start)
            {
                // this.Log().Trace($"filled cache, update emptyBefore.End: {S(_emptyBefore.End)} -> {S(Start)}");
                _emptyBefore.SetEnd(Start);
            }

            if (End < end)
            {
                // this.Log().Trace($"filled cache, update emptyAfter.Start: {S(_emptyAfter.Start)} -> {S(End)}");
                _emptyAfter.SetStart(End);
            }
        }
    }

    private void SyncCache(Instant start, Instant end)
    {
        var (min, max) = GetBounds(start, end, CacheZone);
        // this.Log().Trace($"set {S(min)} - {S(max)} for {S(Start)} - {S(End)}");

        var index = _cache.FindIndex(x => x.Moment == min);
        if (index > 0)
        {
            // this.Log().Trace($"MM: {S(Min)} - {S(Max)}. Cache: {S(Start)} - {S(End)}. Bounds {S(min)} - {S(max)}. Remove range (0, {index}) from {_cache.Count} items");
            _cache.RemoveRange(0, index);
        }

        index = _cache.FindLastIndex(x => x.Moment == max);
        if (index > 0 && index < _cache.Count - 1)
        {
            // this.Log().Trace($"MM: {S(Min)} - {S(Max)}. Cache: {S(Start)} - {S(End)}. Bounds {S(min)} - {S(max)}. Remove range ({index}, {_cache.Count - index}) from {_cache.Count} items");
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
            if (diff != Duration.FromMinutes(1))
                throw new InvalidOperationException($"Cache integrity failure: {_cache[i - 1]}, {_cache[i]}. Diff: {diff}");
        }
    }

    private (Instant, Instant) GetBounds(Instant start, Instant end, long zone)
    {
        var size = Duration.FromTicks((end - start).TotalTicks.FloorInt64() * zone);

        var min = Instant.Max(start - size, Bounds.Start);
        var max = Instant.Min(end + size, Bounds.End);

        return (min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BeginLoad() => Volatile.Write(ref _isLoading, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndLoad() => Volatile.Write(ref _isLoading, 0);

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            throw new InvalidOperationException($"{GetType().FriendlyName()} is already disposed");

        _load = null!;
        _cache.Clear();
    }
}