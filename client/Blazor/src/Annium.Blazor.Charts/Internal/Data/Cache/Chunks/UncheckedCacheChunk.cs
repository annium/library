using System.Collections.Generic;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

internal sealed record UncheckedCacheChunk<T> : CacheChunkBase<T>
{
    public UncheckedCacheChunk(
        Instant start,
        Instant end,
        IReadOnlyCollection<T> items,
        IComparer<T> comparer
    ) : base(start, end, items, comparer)
    {
    }
}