using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Comparers;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

/// <summary>
/// A cache chunk implementation that performs validation on time series data to ensure
/// all items fall within the specified time range boundaries.
/// </summary>
/// <typeparam name="T">The time series data type that implements ITimeSeries</typeparam>
internal sealed record CheckedCacheChunk<T> : CacheChunkBase<T>
    where T : ITimeSeries
{
    /// <summary>
    /// Initializes a new instance of the CheckedCacheChunk class with validation of item boundaries.
    /// </summary>
    /// <param name="start">The start time of the chunk's range</param>
    /// <param name="end">The end time of the chunk's range</param>
    /// <param name="items">The collection of time series items to store in the chunk</param>
    public CheckedCacheChunk(Instant start, Instant end, IReadOnlyCollection<T> items)
        : base(start, end, items, TimeSeriesComparer<T>.Default)
    {
        Validate();
    }

    /// <summary>
    /// Validates that all items in the chunk fall within the specified time range boundaries.
    /// </summary>
    private void Validate()
    {
        if (Items.Count == 0)
            return;

        var first = Items[0];
        if (first.Moment < Range.Start)
            throw new InvalidOperationException(
                $"Invalid chunk: {first} at {first.Moment.S()} goes before start at {Range.Start.S()}"
            );

        var last = Items[^1];
        if (last.Moment > Range.End)
            throw new InvalidOperationException(
                $"Invalid chunk: {last} at {last.Moment.S()} goes after end at {Range.End.S()}"
            );
    }
}
