using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

public interface ISeriesSource<T> : ISeriesSource, IDisposable
{
    bool GetItems(Instant start, Instant end, out IReadOnlyList<T> data);
    T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact);
    void LoadItems(Instant start, Instant end, Action onLoad);
}

public interface ISeriesSource
{
    event Action OnBoundsChange;
    Duration Resolution { get; }
    ValueRange<Instant> Bounds { get; }
    bool IsLoading { get; }
    void SetResolution(Duration resolution);
    void Clear();
}