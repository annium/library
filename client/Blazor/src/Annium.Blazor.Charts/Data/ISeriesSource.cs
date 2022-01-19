using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Data;

public interface ISeriesSource<T> : ISeriesSource, IDisposable
    where T : ITimeSeries
{
    void Init(Func<Instant, Instant, Task<IReadOnlyList<T>>> load);
    bool GetData(Instant start, Instant end, out IReadOnlyList<T> data);
    void LoadData(Instant start, Instant end, Action onLoad);
}

public interface ISeriesSource
{
    bool IsLoading { get; }
    ValueRange<Instant> Bounds { get; }
}