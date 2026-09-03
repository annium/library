namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents an item with a range defined by low and high decimal values.
/// </summary>
public interface IRangeItem
{
    /// <summary>
    /// Gets the low value of the range.
    /// </summary>
    decimal Low { get; }

    /// <summary>
    /// Gets the high value of the range.
    /// </summary>
    decimal High { get; }
}
