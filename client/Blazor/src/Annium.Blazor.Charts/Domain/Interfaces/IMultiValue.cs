using System.Collections.Generic;

namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents a time series data point that contains multiple values of type T.
/// </summary>
/// <typeparam name="T">The type of values contained in this multi-value data point.</typeparam>
public interface IMultiValue<out T> : ITimeSeries
{
    /// <summary>
    /// Gets the collection of items for this data point.
    /// </summary>
    IReadOnlyCollection<T> Items { get; }
}
