namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents a single value with time series functionality.
/// </summary>
/// <typeparam name="T">The type of the item value.</typeparam>
public interface ISingleValue<out T> : ITimeSeries
{
    /// <summary>
    /// Gets the item value.
    /// </summary>
    T Item { get; }
}
