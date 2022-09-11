using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record MultiRangeValue<T>(Instant Moment, IReadOnlyCollection<T> Values) : IMultiValue<T>, IComparable<MultiRangeValue<T>>, IComparable<Instant>
    where T : RangeItem
{
    public int CompareTo(MultiRangeValue<T>? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}