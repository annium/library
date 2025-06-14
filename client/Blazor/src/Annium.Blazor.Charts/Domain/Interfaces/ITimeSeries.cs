using NodaTime;

namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents a data point in a time series with a specific moment in time.
/// </summary>
public interface ITimeSeries
{
    /// <summary>
    /// Gets the specific moment in time for this data point.
    /// </summary>
    Instant Moment { get; }
}
