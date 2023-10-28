using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Comparers;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

internal sealed record CheckedCacheChunk<T> : CacheChunkBase<T>
    where T : ITimeSeries
{
    public CheckedCacheChunk(Instant start, Instant end, IReadOnlyCollection<T> items)
        : base(start, end, items, TimeSeriesComparer<T>.Default)
    {
        Validate();
    }

    private void Validate()
    {
        if (Items.Count == 0)
            return;

        var first = Items[0];
        if (first.Moment < Range.Start)
            throw new InvalidOperationException(
                $"Invalid chunk: {first} at {first.Moment.S()} goes before start at {Range.Start.S()}"
            );

        var last = Items[^1];
        if (last.Moment > Range.End)
            throw new InvalidOperationException(
                $"Invalid chunk: {last} at {last.Moment.S()} goes after end at {Range.End.S()}"
            );
    }
}
