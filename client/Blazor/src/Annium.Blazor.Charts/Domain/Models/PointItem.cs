using Annium.Blazor.Charts.Domain.Interfaces;

namespace Annium.Blazor.Charts.Domain.Models;

/// <summary>
/// Represents a point item with a single decimal value.
/// </summary>
/// <param name="Value">The decimal value of the point item.</param>
public record PointItem(decimal Value) : IPointItem;
