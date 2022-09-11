using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;

namespace Annium.Blazor.Charts.Internal.Data.Comparers;

internal class TimeSeriesComparer<T> : IComparer<T>
    where T : ITimeSeries
{
    public static IComparer<T> Default { get; } = new TimeSeriesComparer<T>();

    private TimeSeriesComparer()
    {
    }

    public int Compare(T? x, T? y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException($"Can't compare null values of {nameof(ITimeSeries)} implementations");

        return x.Moment.CompareTo(y.Moment);
    }
}