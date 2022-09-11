using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal sealed record CheckedCacheChunk<T> : CacheChunkBase<T>
    where T : ITimeSeries, IComparable<T>
{
    public CheckedCacheChunk(Instant start, Instant end, IReadOnlyCollection<T> items) : base(start, end, items)
    {
        Validate();
    }

    private void Validate()
    {
        if (Items.Count == 0)
            return;

        if (Items[0].Moment < Range.Start)
            throw new InvalidOperationException($"Invalid chunk: {Items[0]} goes before start at {Range.Start.S()}");

        if (Items[^1].Moment > Range.End)
            throw new InvalidOperationException($"Invalid chunk: {Items[^1]} goes after end at {Range.End.S()}");
    }
}