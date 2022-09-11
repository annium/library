using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

internal interface ISeriesSourceCache<T>
    where T : IComparable<T>
{
    event Action<ValueRange<Instant>> OnBoundsChange;
    bool IsEmpty { get; }
    ValueRange<Instant> Bounds { get; }
    bool HasData(Instant start, Instant end);
    IReadOnlyList<T> GetData(Instant start, Instant end);
    T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact);
    IReadOnlyList<ValueRange<Instant>> GetEmptyRanges(Instant start, Instant end);
    void AddData(Instant start, Instant end, IReadOnlyCollection<T> data);
    void SetResolution(Duration resolution);
    void Clear();
}