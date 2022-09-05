using System;
using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public abstract record TimeSeries<T>(Instant Moment) : ITimeSeries, IComparable<T>
    where T : ITimeSeries
{
    public int CompareTo(T? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
}