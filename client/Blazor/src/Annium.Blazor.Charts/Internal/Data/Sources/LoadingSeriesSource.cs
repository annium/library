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
using Annium.Logging;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

internal class LoadingSeriesSource<T> : ISeriesSource<T>, ILogSubject
{
    public event Action Loaded = delegate { };
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };
    public ILogger Logger { get; }
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
        ILogger logger
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

        this.Trace<string>("start for {info}", info);

        // no data was attempted to load
        if (_cache.IsEmpty)
        {
            this.Trace<string>("cache is empty for {info}", info);
            data = [];

            return false;
        }

        if (_cache.HasData(min, max))
        {
            this.Trace<string>("all data is available for {info}", info);
            data = _cache.GetData(start, end);

            return true;
        }

        this.Trace<string>("some data is missing for {info}", info);
        data = [];

        return false;
    }

    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    public void LoadItems(Instant start, Instant end)
    {
#pragma warning disable VSTHRD110
        LoadDataAsync(start, end)
            .ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    Loaded();
                else
                {
                    this.Error("load in {start} - {end} failed in {status} status", start.S(), end.S(), t.Status);
                    if (t.Exception is not null)
                        this.Error(t.Exception);
                }
            });
#pragma warning restore VSTHRD110
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

    private async Task LoadDataAsync(Instant start, Instant end)
    {
        Volatile.Write(ref _isLoading, 1);

        var (min, max) = GetBounds(start, end, _resolutionOptions.LoadZone);
        var info = $"{start.S()} - {end.S()} (as {min.S()} - {max.S()})";

        this.Trace<string>("start for {info}", info);

        var emptyRanges = _cache.GetEmptyRanges(min, max);
        var dataset = await Task.WhenAll(
            emptyRanges.Select(async range => (range, await LoadInRangeAsync(range.Start, range.End)))
        );

        foreach (var (range, data) in dataset)
        {
            this.Trace<int, string, string>(
                "save {dataCount} item(s) to cache for {rangeStart} - {rangeEnd}",
                data.Count,
                range.Start.S(),
                range.End.S()
            );
            _cache.AddData(range.Start, range.End, data);
        }

        this.Trace<string>("done for {info}", info);

        Volatile.Write(ref _isLoading, 0);
    }

    private async Task<IReadOnlyList<T>> LoadInRangeAsync(Instant start, Instant end)
    {
        var info = $"{start.S()} - {end.S()}";
        this.Trace<string>("start for {info}", info);

        var items = await _load(Resolution, start, end);

        this.Trace(items.Count > 0 ? $"loaded {items.Count} item(s) for {info}" : $"no items loaded for {info}");

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
