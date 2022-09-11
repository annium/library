using System;
using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record PointValue(Instant Moment, decimal Value) : ITimeSeries, IComparable<PointValue>, IComparable<Instant>
{
    public int CompareTo(PointValue? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}