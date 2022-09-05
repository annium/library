using System;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal sealed class UncheckedSeriesSourceCache<T> : SeriesSourceCacheBase<UncheckedCacheChunk<T>, T>
    where T : IComparable<T>, IComparable<Instant>
{
    public UncheckedSeriesSourceCache(
        Duration resolution
    ) : base(
        resolution,
        (start, end, data) => new UncheckedCacheChunk<T>(start, end, data),
        (item, moment) => item.CompareTo(moment),
        chunk => chunk.Range.Start,
        chunk => chunk.Range.End
    )
    {
    }

    protected override void PostProcessDataChange()
    {
    }
}