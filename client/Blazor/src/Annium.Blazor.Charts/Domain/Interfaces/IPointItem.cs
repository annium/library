namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents a single data point item with a numeric value.
/// </summary>
public interface IPointItem
{
    /// <summary>
    /// Gets the numeric value of this data point item.
    /// </summary>
    decimal Value { get; }
}
