using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record MultiPointValue<T>(Instant Moment, IReadOnlyCollection<T> Values) : IMultiValue<T>, IComparable<MultiPointValue<T>>, IComparable<Instant>
    where T : PointItem
{
    public int CompareTo(MultiPointValue<T>? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}