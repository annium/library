using System;
using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record PlainValue(Instant Moment, decimal Value) : ITimeSeries, IComparable<PlainValue>, IComparable<Instant>
{
    public int CompareTo(PlainValue? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}