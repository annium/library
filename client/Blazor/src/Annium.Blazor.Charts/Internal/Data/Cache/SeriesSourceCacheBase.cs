using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

/// <summary>
/// Base class for series source caches that manage time-based data storage and retrieval.
/// Provides common functionality for chunk-based caching, data lookup, and cache optimization.
/// </summary>
/// <typeparam name="TChunk">The type of cache chunk used to store data</typeparam>
/// <typeparam name="T">The type of data items stored in the cache</typeparam>
internal abstract class SeriesSourceCacheBase<TChunk, T> : ISeriesSourceCache<T>
    where TChunk : CacheChunkBase<T>
{
    /// <summary>
    /// Event raised when the cache bounds change.
    /// </summary>
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };

    /// <summary>
    /// Gets a value indicating whether the cache is empty (contains no chunks).
    /// </summary>
    public bool IsEmpty => Chunks.Count == 0;

    /// <summary>
    /// Gets the overall time bounds of all cached data.
    /// </summary>
    public ValueRange<Instant> Bounds => _bounds;

    /// <summary>
    /// The time resolution for data points in the cache.
    /// </summary>
    protected Duration Resolution;

    /// <summary>
    /// Gets the collection of cache chunks ordered by time.
    /// </summary>
    protected IList<TChunk> Chunks { get; } = new List<TChunk>();

    /// <summary>
    /// Managed bounds for the entire cache.
    /// </summary>
    private readonly ManagedValueRange<Instant> _bounds;

    /// <summary>
    /// Function to create new cache chunks.
    /// </summary>
    private readonly Func<Instant, Instant, IReadOnlyCollection<T>, TChunk> _createChunk;

    /// <summary>
    /// Function to compare data items with time instants.
    /// </summary>
    private readonly Func<T, Instant, int> _compare;

    /// <summary>
    /// Function to get the start time of a chunk.
    /// </summary>
    private readonly Func<TChunk, Instant> _getStart;

    /// <summary>
    /// Function to get the end time of a chunk.
    /// </summary>
    private readonly Func<TChunk, Instant> _getEnd;

    /// <summary>
    /// Initializes a new instance of the SeriesSourceCacheBase class with the specified configuration.
    /// </summary>
    /// <param name="resolution">The time resolution for data points in the cache</param>
    /// <param name="createChunk">Function to create new cache chunks</param>
    /// <param name="compare">Function to compare data items with time instants</param>
    /// <param name="getStart">Function to get the start time of a chunk</param>
    /// <param name="getEnd">Function to get the end time of a chunk</param>
    protected SeriesSourceCacheBase(
        Duration resolution,
        Func<Instant, Instant, IReadOnlyCollection<T>, TChunk> createChunk,
        Func<T, Instant, int> compare,
        Func<TChunk, Instant> getStart,
        Func<TChunk, Instant> getEnd
    )
    {
        Resolution = resolution;
        _createChunk = createChunk;
        _compare = compare;
        _getStart = getStart;
        _getEnd = getEnd;
        _bounds = ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);
    }

    /// <summary>
    /// Determines whether the cache contains data for the specified time range.
    /// </summary>
    /// <param name="start">The start time of the range to check</param>
    /// <param name="end">The end time of the range to check</param>
    /// <returns>True if the cache contains data for the entire specified range; otherwise, false</returns>
    public bool HasData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return true;

        return false;
    }

    /// <summary>
    /// Retrieves all data items within the specified time range.
    /// </summary>
    /// <param name="start">The start time of the range to retrieve</param>
    /// <param name="end">The end time of the range to retrieve</param>
    /// <returns>A read-only list of data items within the specified time range</returns>
    public IReadOnlyList<T> GetData(Instant start, Instant end)
    {
        var range = ValueRange.Create(start, end);

        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(range, RangeBounds.Both))
                return chunk.Items.Where(x => _compare(x, start) >= 0 && _compare(x, end) <= 0).ToArray();

        return [];
    }

    /// <summary>
    /// Retrieves a specific data item at or near the specified time instant.
    /// </summary>
    /// <param name="moment">The time instant to search for</param>
    /// <param name="match">The lookup match strategy to use</param>
    /// <returns>The data item at or near the specified time instant, or null if not found</returns>
    public T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact)
    {
        foreach (var chunk in Chunks)
            if (chunk.Range.Contains(moment, RangeBounds.Both))
                return chunk.Items.Count > 0 ? GetChunkItem(chunk, moment, match) : default;

        return default;
    }

    /// <summary>
    /// Identifies time ranges within the specified period that are not covered by cached data.
    /// </summary>
    /// <param name="start">The start time of the period to analyze</param>
    /// <param name="end">The end time of the period to analyze</param>
    /// <returns>A read-only list of time ranges that are not covered by cached data</returns>
    public IReadOnlyList<ValueRange<Instant>> GetEmptyRanges(Instant start, Instant end)
    {
        var ranges = new List<ValueRange<Instant>>();

        if (Chunks.Count == 0)
        {
            ranges.Add(ValueRange.Create(start, end));
            return ranges;
        }

        var from = start;

        foreach (var chunk in Chunks)
        {
            if (chunk.Range.Start >= end)
                break;

            if (chunk.Range.Start > from)
                ranges.Add(ValueRange.Create(from, chunk.Range.Start - Resolution));

            from = chunk.Range.End + Resolution;
        }

        if (end > from)
            ranges.Add(ValueRange.Create(from, end));

        return ranges;
    }

    /// <summary>
    /// Adds new data to the cache for the specified time range.
    /// </summary>
    /// <param name="start">The start time of the data range</param>
    /// <param name="end">The end time of the data range</param>
    /// <param name="data">The collection of data items to add</param>
    public void AddData(Instant start, Instant end, IReadOnlyCollection<T> data)
    {
        var newChunk = _createChunk(start, end, data);

        if (Chunks.Count == 0)
        {
            Chunks.Add(newChunk);
            SyncBounds();
            PostProcessDataChange();

            return;
        }

        var isAdded = false;
        for (var i = 0; i < Chunks.Count; i++)
        {
            var chunk = Chunks[i];
            if (
                chunk.Range.Contains(newChunk.Range.Start, RangeBounds.Both)
                || chunk.Range.Contains(newChunk.Range.End, RangeBounds.Both)
            )
                throw new InvalidOperationException(
                    $"New chunk {newChunk.Range} intersects with existing chunk {chunk}"
                );

            if (newChunk.Range.End > chunk.Range.Start)
                continue;

            Chunks.Insert(i, newChunk);
            isAdded = true;
            break;
        }

        if (!isAdded)
            Chunks.Add(newChunk);

        Optimize();
        SyncBounds();
        PostProcessDataChange();
    }

    /// <summary>
    /// Sets a new time resolution for the cache, clearing all existing data.
    /// </summary>
    /// <param name="resolution">The new time resolution to use</param>
    public void SetResolution(Duration resolution)
    {
        if (resolution == Resolution)
            return;

        Resolution = resolution;
        Chunks.Clear();
        ResetBounds();
    }

    /// <summary>
    /// Clears all cached data and resets the cache bounds.
    /// </summary>
    public void Clear()
    {
        Chunks.Clear();
        ResetBounds();
    }

    /// <summary>
    /// Performs post-processing operations after data changes. Override in derived classes to implement specific behaviors.
    /// </summary>
    protected abstract void PostProcessDataChange();

    /// <summary>
    /// Synchronizes the cache bounds with the actual data range.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SyncBounds() => SetBounds(_getStart(Chunks[0]), _getEnd(Chunks[^1]));

    /// <summary>
    /// Resets the cache bounds to the Unix epoch.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetBounds() => SetBounds(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

    /// <summary>
    /// Sets the cache bounds and raises the bounds change event if the bounds have changed.
    /// </summary>
    /// <param name="start">The new start bound</param>
    /// <param name="end">The new end bound</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBounds(Instant start, Instant end)
    {
        if (start == _bounds.Start && end == _bounds.End)
            return;

        _bounds.Set(start, end);
        OnBoundsChange(_bounds);
    }

    /// <summary>
    /// Optimizes the cache by merging adjacent chunks that are contiguous in time.
    /// </summary>
    private void Optimize()
    {
        var i = 0;

        while (i < Chunks.Count - 1)
        {
            var current = Chunks[i];
            var next = Chunks[i + 1];

            if (next.Range.Start - current.Range.End == Resolution)
            {
                current.Append(next);
                Chunks.RemoveAt(i + 1);
            }
            else
                i++;
        }
    }

    /// <summary>
    /// Retrieves a specific item from a chunk using binary search with the specified lookup match strategy.
    /// </summary>
    /// <param name="chunk">The chunk to search in</param>
    /// <param name="moment">The time instant to search for</param>
    /// <param name="match">The lookup match strategy to use</param>
    /// <returns>The found item or null if not found according to the match strategy</returns>
    private T? GetChunkItem(TChunk chunk, Instant moment, LookupMatch match)
    {
        var items = chunk.Items;

        if (_compare(items[0], moment) > 0)
            return match switch
            {
                LookupMatch.NearestRight => items[0],
                _ => default,
            };

        if (_compare(items[^1], moment) < 0)
            return match switch
            {
                LookupMatch.NearestLeft => items[^1],
                _ => default,
            };

        var l = 0;
        var r = items.Count - 1;

        while (l <= r)
        {
            var i = ((r - l) / 2m).FloorInt32().Within(l, r);
            var item = items[i];

            if (_compare(item, moment) < 0)
                l = i + 1;
            else if (_compare(item, moment) > 0)
                r = i - 1;
            else
                return item;
        }

        return match switch
        {
            LookupMatch.NearestLeft => r >= 0 ? items[r] : default,
            LookupMatch.NearestRight => l < items.Count ? items[l] : default,
            _ => default,
        };
    }
}
