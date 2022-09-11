using System;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

public record Candle(
    Instant Moment,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : ITimeSeries, IComparable<Candle>, IComparable<Instant>
{
    public int CompareTo(Candle? other) => Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}