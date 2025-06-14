using Annium.Blazor.Charts.Domain.Interfaces;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a range item with low and high decimal values.
/// </summary>
/// <param name="Low">The lower bound of the range.</param>
/// <param name="High">The upper bound of the range.</param>
public record RangeItem(decimal Low, decimal High) : IRangeItem;
