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

/// <summary>
/// A series source that loads data asynchronously from external sources
/// </summary>
/// <typeparam name="T">The type of data items in the series</typeparam>
internal class LoadingSeriesSource<T> : ISeriesSource<T>, ILogSubject
{
    /// <summary>
    /// Event raised when data loading is completed
    /// </summary>
    public event Action Loaded = delegate { };

    /// <summary>
    /// Event raised when the data bounds change
    /// </summary>
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };

    /// <summary>
    /// Gets the logger instance for this series source
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the time resolution of the series data
    /// </summary>
    public Duration Resolution { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the series source is currently loading data
    /// </summary>
    public bool IsLoading => Volatile.Read(ref _isLoading) == 1;

    /// <summary>
    /// Gets the time bounds of the available data
    /// </summary>
    public ValueRange<Instant> Bounds => _cache.Bounds;

    /// <summary>
    /// The cache for storing loaded data
    /// </summary>
    private readonly ISeriesSourceCache<T> _cache;

    /// <summary>
    /// Function that loads data from external sources
    /// </summary>
    private readonly Func<Duration, Instant, Instant, Task<IReadOnlyList<T>>> _load;

    /// <summary>
    /// Configuration options for the series source
    /// </summary>
    private readonly ISeriesSourceOptions _options;

    /// <summary>
    /// Resolution-specific options for the series source
    /// </summary>
    private SeriesSourceResolutionOptions _resolutionOptions;

    /// <summary>
    /// Flag indicating whether the source is currently loading data (used with Interlocked operations)
    /// </summary>
    private int _isLoading;

    /// <summary>
    /// Tail of the serialized load chain: each <see cref="LoadItems"/> continues after the previous load completes,
    /// so overlapping loads recompute their empty ranges against an up-to-date cache instead of colliding.
    /// </summary>
    private Task _loading = Task.CompletedTask;

    /// <summary>
    /// Flag indicating whether the source has been disposed (used with Interlocked operations)
    /// </summary>
    private int _isDisposed;

    /// <summary>
    /// Initializes a new instance of the LoadingSeriesSource class
    /// </summary>
    /// <param name="cache">The cache for storing loaded data</param>
    /// <param name="resolution">The time resolution for the series data</param>
    /// <param name="load">Function that loads data from external sources</param>
    /// <param name="options">Configuration options for the series source</param>
    /// <param name="logger">Logger instance for this series source</param>
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

    /// <summary>
    /// Gets data items within the specified time range
    /// </summary>
    /// <param name="start">The start time of the range</param>
    /// <param name="end">The end time of the range</param>
    /// <param name="data">The retrieved data items</param>
    /// <returns>True if data was successfully retrieved, false otherwise</returns>
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

    /// <summary>
    /// Gets a single data item at the specified moment in time
    /// </summary>
    /// <param name="moment">The moment in time to look up</param>
    /// <param name="match">The lookup match strategy</param>
    /// <returns>The data item if found, null otherwise</returns>
    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    /// <summary>
    /// Initiates asynchronous loading of data items within the specified time range
    /// </summary>
    /// <param name="start">The start time of the range to load</param>
    /// <param name="end">The end time of the range to load</param>
    public void LoadItems(Instant start, Instant end)
    {
        // Serialize loads: chain each load after the previous so overlapping calls recompute their empty ranges
        // against an up-to-date cache instead of colliding in AddData. Blazor WASM is single-threaded, so the
        // read-modify-write of _loading needs no lock.
        _loading = LoadSerializedAsync(_loading, start, end);
    }

    /// <summary>
    /// Awaits the previous load, then loads the requested range and raises <see cref="Loaded"/> on success.
    /// Failures are logged, not rethrown, so one failed load does not break the serialized chain.
    /// </summary>
    /// <param name="previous">The previous load in the chain to await before starting this one.</param>
    /// <param name="start">The start time of the range to load.</param>
    /// <param name="end">The end time of the range to load.</param>
    /// <returns>A task representing the serialized load.</returns>
    private async Task LoadSerializedAsync(Task previous, Instant start, Instant end)
    {
        try
        {
            // VSTHRD003: `previous` is our own prior load task, started in this same (single-threaded) context —
            // awaiting it to serialize loads is safe, not the cross-context await the analyzer warns about.
#pragma warning disable VSTHRD003
            await previous;
#pragma warning restore VSTHRD003
        }
        catch
        {
            // a previous load's failure was already logged by its own invocation; don't let it break the chain
        }

        try
        {
            await LoadDataAsync(start, end);
            Loaded();
        }
        catch (Exception e)
        {
            this.Error<string, string>("load in {start} - {end} failed", start.S(), end.S());
            this.Error(e);
        }
    }

    /// <summary>
    /// Sets the time resolution for the series data
    /// </summary>
    /// <param name="resolution">The new resolution to set</param>
    public void SetResolution(Duration resolution)
    {
        if (resolution == Resolution)
            return;

        Resolution = resolution;
        _resolutionOptions = _options.GetForResolution(resolution);
        _cache.SetResolution(resolution);
    }

    /// <summary>
    /// Clears all cached data from the series source
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Asynchronously loads data for the specified time range
    /// </summary>
    /// <param name="start">The start time of the range to load</param>
    /// <param name="end">The end time of the range to load</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task LoadDataAsync(Instant start, Instant end)
    {
        Volatile.Write(ref _isLoading, 1);

        try
        {
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
        }
        finally
        {
            Volatile.Write(ref _isLoading, 0);
        }
    }

    /// <summary>
    /// Asynchronously loads data for a specific time range
    /// </summary>
    /// <param name="start">The start time of the range</param>
    /// <param name="end">The end time of the range</param>
    /// <returns>A task containing the loaded data items</returns>
    private async Task<IReadOnlyList<T>> LoadInRangeAsync(Instant start, Instant end)
    {
        var info = $"{start.S()} - {end.S()}";
        this.Trace<string>("start for {info}", info);

        var items = await _load(Resolution, start, end);

        if (items.Count > 0)
            this.Trace<int, string>("loaded {count} item(s) for {info}", items.Count, info);
        else
            this.Trace<string>("no items loaded for {info}", info);

        return items;
    }

    /// <summary>
    /// Calculates the expanded time bounds based on the specified zone factor
    /// </summary>
    /// <param name="start">The start time</param>
    /// <param name="end">The end time</param>
    /// <param name="zone">The zone factor for expanding the bounds</param>
    /// <returns>A tuple containing the expanded start and end times</returns>
    private (Instant, Instant) GetBounds(Instant start, Instant end, decimal zone)
    {
        var range = (end - start).TotalMinutes.CeilInt64();
        var size = Duration.FromMinutes((range * zone).CeilInt64()).CeilTo(Resolution);

        return (start - size, end + size);
    }

    /// <summary>
    /// Disposes the series source and cleans up resources
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            throw new InvalidOperationException($"{this.GetFullId()} is already disposed");

        _cache.Clear();
    }
}
