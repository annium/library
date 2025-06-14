using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache;

/// <summary>
/// Provides caching functionality for time series data with efficient range-based operations
/// </summary>
/// <typeparam name="T">The type of time series data items to cache</typeparam>
internal interface ISeriesSourceCache<T>
{
    /// <summary>
    /// Occurs when the time bounds of the cached data change
    /// </summary>
    event Action<ValueRange<Instant>> OnBoundsChange;

    /// <summary>
    /// Gets a value indicating whether the cache contains no data
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets the time range covered by all cached data
    /// </summary>
    ValueRange<Instant> Bounds { get; }

    /// <summary>
    /// Determines whether data is available for the specified time range
    /// </summary>
    /// <param name="start">The start time of the range to check</param>
    /// <param name="end">The end time of the range to check</param>
    /// <returns>True if data is available for the entire range; otherwise, false</returns>
    bool HasData(Instant start, Instant end);

    /// <summary>
    /// Retrieves cached data items within the specified time range
    /// </summary>
    /// <param name="start">The start time of the range to retrieve</param>
    /// <param name="end">The end time of the range to retrieve</param>
    /// <returns>A read-only list of data items within the specified range</returns>
    IReadOnlyList<T> GetData(Instant start, Instant end);

    /// <summary>
    /// Attempts to find a specific data item at or near the specified moment
    /// </summary>
    /// <param name="moment">The target time moment</param>
    /// <param name="match">The lookup matching strategy to use</param>
    /// <returns>The matching data item, or null if no match is found</returns>
    T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact);

    /// <summary>
    /// Identifies time ranges within the specified period that have no cached data
    /// </summary>
    /// <param name="start">The start time of the range to analyze</param>
    /// <param name="end">The end time of the range to analyze</param>
    /// <returns>A read-only list of time ranges that are not covered by cached data</returns>
    IReadOnlyList<ValueRange<Instant>> GetEmptyRanges(Instant start, Instant end);

    /// <summary>
    /// Adds new data items to the cache for the specified time range
    /// </summary>
    /// <param name="start">The start time of the data range</param>
    /// <param name="end">The end time of the data range</param>
    /// <param name="data">The collection of data items to cache</param>
    void AddData(Instant start, Instant end, IReadOnlyCollection<T> data);

    /// <summary>
    /// Sets the time resolution for cache operations and optimization
    /// </summary>
    /// <param name="resolution">The time resolution to use</param>
    void SetResolution(Duration resolution);

    /// <summary>
    /// Removes all cached data and resets the cache to an empty state
    /// </summary>
    void Clear();
}
