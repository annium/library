using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal sealed class CheckedSeriesSourceCache<T> : SeriesSourceCacheBase<CheckedCacheChunk<T>, T>
    where T : ITimeSeries
{
    public CheckedSeriesSourceCache(Duration resolution)
        : base(
            resolution,
            (start, end, data) => new CheckedCacheChunk<T>(start, end, data),
            (item, moment) => item.Moment.CompareTo(moment),
            chunk => chunk.Items[0].Moment,
            chunk => chunk.Items[^1].Moment
        ) { }

    protected override void PostProcessDataChange()
    {
        ValidateCacheIntegrity();
    }

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
