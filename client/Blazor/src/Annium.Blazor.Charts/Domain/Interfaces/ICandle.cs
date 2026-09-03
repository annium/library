namespace Annium.Blazor.Charts.Domain.Interfaces;

/// <summary>
/// Represents a candlestick chart data point with open, high, low, and close values for a specific time period.
/// </summary>
public interface ICandle : ITimeSeries
{
    /// <summary>
    /// Gets the opening price for the time period.
    /// </summary>
    decimal Open { get; }

    /// <summary>
    /// Gets the highest price during the time period.
    /// </summary>
    decimal High { get; }

    /// <summary>
    /// Gets the lowest price during the time period.
    /// </summary>
    decimal Low { get; }

    /// <summary>
    /// Gets the closing price for the time period.
    /// </summary>
    decimal Close { get; }
}
