using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Data.Models;
using Annium.Logging.Abstractions;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class LoadingSeriesSource<T> : ISeriesSource<T>, ILogSubject<LoadingSeriesSource<T>>
{
    public event Action Loaded = delegate { };
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public ILogger<LoadingSeriesSource<T>> Logger { get; }
    public Duration Resolution { get; private set; }
    public bool IsLoading => Volatile.Read(ref _isLoading) == 1;
    public ValueRange<Instant> Bounds => _cache.Bounds;
    private readonly ISeriesSourceCache<T> _cache;
    private readonly Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> _load;
    private readonly ISeriesSourceOptions _options;
    private SeriesSourceResolutionOptions _resolutionOptions;
    private int _isLoading;
    private int _isDisposed;

    public LoadingSeriesSource(
        ISeriesSourceCache<T> cache,
        Duration resolution,
        Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> load,
        ISeriesSourceOptions options,
        ILogger<LoadingSeriesSource<T>> logger
    )
    {
        Resolution = resolution;
        Logger = logger;
        _cache = cache;
        _load = load;
        _options = options;
        _resolutionOptions = options.GetForResolution(resolution);
        _cache.OnBoundsChange += bounds => OnBoundsChange(bounds);
    }

    public bool GetItems(Instant start, Instant end, out IReadOnlyList<T> data)
    {
        var (min, max) = GetBounds(start, end, _resolutionOptions.BufferZone);
        var info = $"{start.S()} - {end.S()} (as {min.S()} - {max.S()})";

        this.Log().Trace($"start for {info}");

        // no data was attempted to load
        if (_cache.IsEmpty)
        {
            this.Log().Trace($"cache is empty for {info}");
            data = Array.Empty<T>();

            return false;
        }

        if (_cache.HasData(min, max))
        {
            this.Log().Trace($"all data is available for {info}");
            data = _cache.GetData(start, end);

            return true;
        }

        this.Log().Trace($"some data is missing for {info}");
        data = Array.Empty<T>();

        return false;
    }

    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    public void LoadItems(Instant start, Instant end)
    {
        LoadData(start, end).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    Loaded();
                else
                {
                    this.Log().Error($"load in {start.S()} - {end.S()} failed in {t.Status} status");
                    if (t.Exception is not null)
                        this.Log().Error(t.Exception);
                }
            }
        );
    }

    public void SetResolution(Duration resolution)
    {
        if (resolution == Resolution)
            return;

        Resolution = resolution;
        _resolutionOptions = _options.GetForResolution(resolution);
        _cache.SetResolution(resolution);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private async Task LoadData(Instant start, Instant end)
    {
        Volatile.Write(ref _isLoading, 1);

        var (min, max) = GetBounds(start, end, _resolutionOptions.LoadZone);
        var info = $"{start.S()} - {end.S()} (as {min.S()} - {max.S()})";

        this.Log().Trace($"start for {info}");

        var emptyRanges = _cache.GetEmptyRanges(min, max);
        var dataset = await Task.WhenAll(emptyRanges.Select(async range => (range, await LoadInRange(range.Start, range.End))));

        foreach (var (range, data) in dataset)
        {
            this.Log().Trace($"save {data.Count} item(s) to cache for {range.Start.S()} - {range.End.S()}");
            _cache.AddData(range.Start, range.End, data);
        }

        this.Log().Trace($"done for {info}");

        Volatile.Write(ref _isLoading, 0);
    }

    private async Task<IReadOnlyList<T>> LoadInRange(Instant start, Instant end)
    {
        var info = $"{start.S()} - {end.S()}";
        this.Log().Trace($"start for {info}");

        var items = await _load(Resolution, start, end);

        this.Log().Trace(items.Count > 0 ? $"loaded {items.Count} item(s) for {info}" : $"no items loaded for {info}");

        return items;
    }

    private (Instant, Instant) GetBounds(Instant start, Instant end, decimal zone)
    {
        var range = (end - start).TotalMinutes.CeilInt64();
        var size = Duration.FromMinutes((range * zone).CeilInt64()).CeilTo(Resolution);

        return (start - size, end + size);
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            throw new InvalidOperationException($"{this.GetFullId()} is already disposed");

        _cache.Clear();
    }
}