using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Data;

public interface ISeriesSource<TData> : ISeriesSource, IDisposable
    where TData : ITimeSeries
{
    bool GetItems(Instant start, Instant end, out IReadOnlyList<TData> data);
    TData? GetItem(Instant moment);
    void LoadItems(Instant start, Instant end, Action onLoad);
}

public interface ISeriesSource
{
    bool IsLoading { get; }
    ValueRange<Instant> Bounds { get; }
}