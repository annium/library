using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Core.Primitives;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class LoadingSeriesSource<T> : ISeriesSource<T>, ILogSubject<LoadingSeriesSource<T>>
    where T : IComparable<T>
{
    public Duration Resolution { get; private set; }
    public bool IsLoading => Volatile.Read(ref _isLoading) == 1;
    public ILogger<LoadingSeriesSource<T>> Logger { get; }

    public ValueRange<Instant> Bounds => _cache.Bounds;

    private readonly ISeriesSourceCache<T> _cache;
    private readonly Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> _load;
    private readonly SeriesSourceOptions _options;
    private int _isLoading;
    private int _isDisposed;

    public LoadingSeriesSource(
        ISeriesSourceCache<T> cache,
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        SeriesSourceOptions options,
        ILogger<LoadingSeriesSource<T>> logger
    )
    {
        Resolution = resolution;
        Logger = logger;
        _cache = cache;
        _load = load;
        _options = options;
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<T> data)
    {
        var (min, max) = GetBounds(start, end, _options.BufferZone);

        this.Log().Trace($"get in {start.S()} - {end.S()} (as {min.S()} - {max.S()})");

        // no data was attempted to load
        if (_cache.IsEmpty)
        {
            this.Log().Trace("cache is empty");
            data = Array.Empty<T>();

            return false;
        }

        if (_cache.HasData(min, max))
        {
            this.Log().Trace($"all data in {min.S()} - {max.S()} is available");
            data = _cache.GetData(start, end);

            return true;
        }

        this.Log().Trace($"some data in {min.S()} - {max.S()} is missing");
        data = Array.Empty<T>();

        return false;
    }

    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

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
        _cache.SetResolution(resolution);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private async Task LoadData(Instant start, Instant end)
    {
        Volatile.Write(ref _isLoading, 1);

        var (min, max) = GetBounds(start, end, _options.LoadZone);

        this.Log().Trace($"start in {start.S()} - {end.S()} (as {min.S()} - {max.S()})");

        var emptyRanges = _cache.GetEmptyRanges(min, max);
        var dataset = await Task.WhenAll(emptyRanges.Select(async range => (range, await LoadInRange(range.Start, range.End))));

        foreach (var (range, data) in dataset)
        {
            this.Log().Trace($"save {data.Count} item(s) in {range.Start.S()} - {range.End.S()} to cache");
            _cache.AddData(range.Start, range.End, data);
        }

        this.Log().Trace($"done in {start.S()} - {end.S()} (as {min.S()} - {max.S()})");

        Volatile.Write(ref _isLoading, 0);
    }

    private async Task<IReadOnlyList<T>> LoadInRange(Instant start, Instant end)
    {
        this.Log().Trace($"{start.S()} - {end.S()}");

        var items = await _load(Resolution, start, end);

        this.Log().Trace(items.Count > 0 ? $"loaded {items.Count} items in {start.S()} - {end.S()}" : "no items loaded");

        return items;
    }

    private (Instant, Instant) GetBounds(Instant start, Instant end, long zone)
    {
        var size = Duration.FromTicks((end - start).TotalTicks.FloorInt64() * zone);

        return (start - size, end + size);
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            throw new InvalidOperationException($"{GetType().FriendlyName()} is already disposed");

        _cache.Clear();
    }
}