using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal sealed record UncheckedCacheChunk<T> : CacheChunkBase<T>
    where T : IComparable<T>, IComparable<Instant>
{
    public UncheckedCacheChunk(Instant start, Instant end, IReadOnlyCollection<T> items) : base(start, end, items)
    {
    }
}