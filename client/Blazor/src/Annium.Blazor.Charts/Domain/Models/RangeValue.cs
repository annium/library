using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a range value with a timestamp and low/high decimal values.
/// </summary>
/// <param name="Moment">The timestamp when the range was recorded.</param>
/// <param name="Low">The lower bound of the range at the specified moment.</param>
/// <param name="High">The upper bound of the range at the specified moment.</param>
public record RangeValue(Instant Moment, decimal Low, decimal High) : IRangeValue;
