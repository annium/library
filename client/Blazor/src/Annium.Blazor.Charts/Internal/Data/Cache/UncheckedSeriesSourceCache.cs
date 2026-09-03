using System;
using Annium.Blazor.Charts.Data.Comparers;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

/// <summary>
/// Provides a cache implementation for series source data without validation checks
/// </summary>
/// <typeparam name="T">The type of data items stored in the cache</typeparam>
internal sealed class UncheckedSeriesSourceCache<T> : SeriesSourceCacheBase<UncheckedCacheChunk<T>, T>
{
    /// <summary>
    /// Initializes a new instance of the UncheckedSeriesSourceCache class
    /// </summary>
    /// <param name="resolution">The time resolution for the cache</param>
    /// <param name="compare">Function to compare two data items</param>
    /// <param name="compareToMoment">Function to compare a data item to a specific moment in time</param>
    public UncheckedSeriesSourceCache(
        Duration resolution,
        Func<T, T, int> compare,
        Func<T, Instant, int> compareToMoment
    )
        : base(
            resolution,
            (start, end, data) => new UncheckedCacheChunk<T>(start, end, data, ItemComparer.For(compare)),
            compareToMoment,
            chunk => chunk.Range.Start,
            chunk => chunk.Range.End
        ) { }

    /// <summary>
    /// Performs post-processing after data changes. No additional processing is performed in unchecked cache
    /// </summary>
    protected override void PostProcessDataChange() { }
}
