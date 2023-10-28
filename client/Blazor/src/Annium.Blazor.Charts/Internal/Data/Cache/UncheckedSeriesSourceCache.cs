using System;
using Annium.Blazor.Charts.Data.Comparers;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal sealed class UncheckedSeriesSourceCache<T> : SeriesSourceCacheBase<UncheckedCacheChunk<T>, T>
{
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

    protected override void PostProcessDataChange() { }
}
