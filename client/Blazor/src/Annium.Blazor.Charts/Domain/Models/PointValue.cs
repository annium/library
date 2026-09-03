using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a point value with a timestamp and decimal value.
/// </summary>
/// <param name="Moment">The timestamp when the value was recorded.</param>
/// <param name="Value">The decimal value at the specified moment.</param>
public record PointValue(Instant Moment, decimal Value) : IPointValue;
