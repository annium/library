using System;
using Annium.Blazor.Charts.Domain.Interfaces;
using NodaTime;

namespace Demo.Blazor.Charts.Domain;

/// <summary>
/// Represents a candlestick data point with OHLC (Open, High, Low, Close) prices at a specific moment in time
/// </summary>
/// <param name="Moment">The timestamp of the candle</param>
/// <param name="Open">The opening price</param>
/// <param name="High">The highest price during the period</param>
/// <param name="Low">The lowest price during the period</param>
/// <param name="Close">The closing price</param>
public record Candle(Instant Moment, decimal Open, decimal High, decimal Low, decimal Close)
    : ICandle,
        IComparable<Candle>,
        IComparable<Instant>
{
    /// <summary>
    /// Compares this candle to another candle based on their moment timestamps
    /// </summary>
    /// <param name="other">The candle to compare to</param>
    /// <returns>A value indicating the relative order of the candles</returns>
    public int CompareTo(Candle? other) =>
        Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));

    /// <summary>
    /// Compares this candle's moment to a specific instant
    /// </summary>
    /// <param name="other">The instant to compare to</param>
    /// <returns>A value indicating the relative order of the timestamps</returns>
    public int CompareTo(Instant other) => Moment.CompareTo(other);
}
