using System.Collections.Generic;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

/// <summary>
/// A cache chunk implementation that stores data items without performing boundary validation.
/// Provides better performance when data integrity is guaranteed by external means.
/// </summary>
/// <typeparam name="T">The type of data items stored in the cache chunk</typeparam>
internal sealed record UncheckedCacheChunk<T> : CacheChunkBase<T>
{
    /// <summary>
    /// Initializes a new instance of the UncheckedCacheChunk class without boundary validation.
    /// </summary>
    /// <param name="start">The start time of the chunk's range</param>
    /// <param name="end">The end time of the chunk's range</param>
    /// <param name="items">The collection of data items to store in the chunk</param>
    /// <param name="comparer">The comparer used to sort items within the chunk</param>
    public UncheckedCacheChunk(Instant start, Instant end, IReadOnlyCollection<T> items, IComparer<T> comparer)
        : base(start, end, items, comparer) { }
}
