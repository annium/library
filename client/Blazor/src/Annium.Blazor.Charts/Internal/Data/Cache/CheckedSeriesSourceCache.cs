using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

/// <summary>
/// A series source cache implementation that performs integrity validation on cached data.
/// Validates that cache chunks are properly structured and data points are correctly spaced according to the specified resolution.
/// </summary>
/// <typeparam name="T">The time series data type that implements ITimeSeries</typeparam>
internal sealed class CheckedSeriesSourceCache<T> : SeriesSourceCacheBase<CheckedCacheChunk<T>, T>
    where T : ITimeSeries
{
    /// <summary>
    /// Initializes a new instance of the CheckedSeriesSourceCache class with the specified resolution.
    /// </summary>
    /// <param name="resolution">The time resolution for data points in the cache</param>
    public CheckedSeriesSourceCache(Duration resolution)
        : base(
            resolution,
            (start, end, data) => new CheckedCacheChunk<T>(start, end, data),
            (item, moment) => item.Moment.CompareTo(moment),
            chunk => chunk.Items[0].Moment,
            chunk => chunk.Items[^1].Moment
        ) { }

    /// <summary>
    /// Performs post-processing after data changes by validating cache integrity.
    /// </summary>
    protected override void PostProcessDataChange()
    {
        ValidateCacheIntegrity();
    }

    /// <summary>
    /// Validates the integrity of the entire cache by checking all chunks for proper structure and data consistency.
    /// </summary>
    private void ValidateCacheIntegrity()
    {
        for (var i = 0; i < Chunks.Count; i++)
        {
            var chunk = Chunks[i];
            var items = chunk.Items;
            if (items.Count == 0)
                throw new InvalidOperationException($"Cache integrity failure: chunk {chunk} is empty");

            ValidateChunkBoundsIntegrity(chunk, i == 0, i == Chunks.Count - 1);
            ValidateChunkItemsIntegrity(items, Resolution);
        }
    }

    /// <summary>
    /// Validates that chunk boundaries align correctly with the actual data points within the chunk.
    /// </summary>
    /// <param name="chunk">The cache chunk to validate</param>
    /// <param name="isFirst">True if this is the first chunk in the cache</param>
    /// <param name="isLast">True if this is the last chunk in the cache</param>
    private void ValidateChunkBoundsIntegrity(CheckedCacheChunk<T> chunk, bool isFirst, bool isLast)
    {
        var items = chunk.Items;
        if (!isFirst && items[0].Moment != chunk.Range.Start)
            throw new InvalidOperationException(
                $"Cache integrity failure: chunk {chunk} first item doesn't match range start"
            );

        if (!isLast && items[^1].Moment != chunk.Range.End)
            throw new InvalidOperationException(
                $"Cache integrity failure: chunk {chunk} last item doesn't match range end"
            );
    }

    /// <summary>
    /// Validates that all items within a chunk are properly spaced according to the specified resolution.
    /// </summary>
    /// <param name="items">The collection of items to validate</param>
    /// <param name="resolution">The expected time resolution between consecutive items</param>
    private void ValidateChunkItemsIntegrity(IReadOnlyList<T> items, Duration resolution)
    {
        for (var i = 1; i < items.Count; i++)
        {
            var diff = items[i].Moment - items[i - 1].Moment;
            if (diff != resolution)
                throw new InvalidOperationException(
                    $"Cache integrity failure: {items[i - 1]}, {items[i]}. Diff: {diff}"
                );
        }
    }
}
