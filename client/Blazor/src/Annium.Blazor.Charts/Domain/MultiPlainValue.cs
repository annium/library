using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Blazor.Charts.Domain;

public record MultiPlainValue<T>(Instant Moment, IReadOnlyCollection<T> Values) : IMultiValue<T>, IComparable<MultiPlainValue<T>>, IComparable<Instant>
    where T : PlainItem
{
    public int CompareTo(MultiPlainValue<T>? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}