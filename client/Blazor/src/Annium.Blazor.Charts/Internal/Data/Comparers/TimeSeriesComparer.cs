using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;

namespace Annium.Blazor.Charts.Internal.Data.Comparers;

/// <summary>
/// Provides comparison functionality for time series data based on their time moments
/// </summary>
/// <typeparam name="T">The type of time series data that implements ITimeSeries</typeparam>
internal class TimeSeriesComparer<T> : IComparer<T>
    where T : ITimeSeries
{
    /// <summary>
    /// Gets the default instance of the time series comparer
    /// </summary>
    public static IComparer<T> Default { get; } = new TimeSeriesComparer<T>();

    /// <summary>
    /// Initializes a new instance of the TimeSeriesComparer class
    /// </summary>
    private TimeSeriesComparer() { }

    /// <summary>
    /// Compares two time series objects based on their time moments
    /// </summary>
    /// <param name="x">The first time series object to compare</param>
    /// <param name="y">The second time series object to compare</param>
    /// <returns>A value indicating the relative order of the objects based on their time moments</returns>
    public int Compare(T? x, T? y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException($"Can't compare null values of {nameof(ITimeSeries)} implementations");

        return x.Moment.CompareTo(y.Moment);
    }
}
