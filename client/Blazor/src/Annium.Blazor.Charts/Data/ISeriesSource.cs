using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Data;

public interface ISeriesSource<T> : ISeriesSource, IDisposable
    where T : ITimeSeries
{
    bool GetItems(Instant start, Instant end, out IReadOnlyList<T> data);
    void LoadItems(Instant start, Instant end, Action onLoad);
}

public interface ISeriesSource
{
    bool IsLoading { get; }
    ValueRange<Instant> Bounds { get; }
}