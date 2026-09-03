using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Data.Sources;

/// <summary>
/// Represents a generic series source that provides time-based data of type T.
/// </summary>
/// <typeparam name="T">The type of data items in the series.</typeparam>
public interface ISeriesSource<T> : ISeriesSource, IDisposable
{
    /// <summary>
    /// Gets items within the specified time range.
    /// </summary>
    /// <param name="start">The start instant of the range.</param>
    /// <param name="end">The end instant of the range.</param>
    /// <param name="data">The collection of items within the range.</param>
    /// <returns>True if items were successfully retrieved; otherwise, false.</returns>
    bool GetItems(Instant start, Instant end, out IReadOnlyList<T> data);

    /// <summary>
    /// Gets a single item at the specified moment using the specified lookup match strategy.
    /// </summary>
    /// <param name="moment">The instant to look up.</param>
    /// <param name="match">The lookup match strategy to use.</param>
    /// <returns>The item at the specified moment, or null if not found.</returns>
    T? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact);

    /// <summary>
    /// Loads items within the specified time range.
    /// </summary>
    /// <param name="start">The start instant of the range to load.</param>
    /// <param name="end">The end instant of the range to load.</param>
    void LoadItems(Instant start, Instant end);
}

/// <summary>
/// Represents a base series source that provides time-based data management functionality.
/// </summary>
public interface ISeriesSource
{
    /// <summary>
    /// Occurs when data has been loaded.
    /// </summary>
    event Action Loaded;

    /// <summary>
    /// Occurs when the bounds of the data change.
    /// </summary>
    event Action<ValueRange<Instant>> OnBoundsChange;

    /// <summary>
    /// Gets the resolution of the data series.
    /// </summary>
    Duration Resolution { get; }

    /// <summary>
    /// Gets the time bounds of the available data.
    /// </summary>
    ValueRange<Instant> Bounds { get; }

    /// <summary>
    /// Gets a value indicating whether data is currently being loaded.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Sets the resolution for the data series.
    /// </summary>
    /// <param name="resolution">The new resolution to set.</param>
    void SetResolution(Duration resolution);

    /// <summary>
    /// Clears all loaded data.
    /// </summary>
    void Clear();
}
