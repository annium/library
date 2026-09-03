using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Extensions;
using Annium.Data.Models;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Cache.Chunks;

/// <summary>
/// Base class for cache chunks that store time-based data items within a specific time range.
/// Provides common functionality for managing data items and their temporal boundaries.
/// </summary>
/// <typeparam name="T">The type of data items stored in the cache chunk</typeparam>
internal abstract record CacheChunkBase<T>
{
    /// <summary>
    /// Comparer used to sort items within the chunk.
    /// </summary>
    private readonly IComparer<T> _comparer;

    /// <summary>
    /// Gets the time range covered by this cache chunk.
    /// </summary>
    public ValueRange<Instant> Range => _range;

    /// <summary>
    /// Gets the collection of data items stored in this chunk.
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// Managed time range that can be modified as the chunk grows.
    /// </summary>
    private readonly ManagedValueRange<Instant> _range;

    /// <summary>
    /// Initializes a new instance of the CacheChunkBase class with the specified time range, items, and comparer.
    /// </summary>
    /// <param name="start">The start time of the chunk's range</param>
    /// <param name="end">The end time of the chunk's range</param>
    /// <param name="items">The collection of data items to store in the chunk</param>
    /// <param name="comparer">The comparer used to sort items within the chunk</param>
    protected CacheChunkBase(Instant start, Instant end, IReadOnlyCollection<T> items, IComparer<T> comparer)
    {
        _comparer = comparer;
        _range = ValueRange.Create(start, end);
        Items = items.OrderBy(x => x, comparer).ToList();
        Validate();
    }

    /// <summary>
    /// Appends another cache chunk to this chunk, extending the range and merging the items.
    /// </summary>
    /// <param name="chunk">The cache chunk to append to this chunk</param>
    public void Append(CacheChunkBase<T> chunk)
    {
        _range.SetEnd(chunk.Range.End);
        Items.AddRange(chunk.Items);
        Items.Sort(_comparer);
    }

    /// <summary>
    /// Validates that the chunk's time range is valid (start is not after end).
    /// </summary>
    private void Validate()
    {
        if (Range.Start > Range.End)
            throw new InvalidOperationException($"Invalid chunk: {Range} is invalid");
    }

    /// <summary>
    /// Returns a string representation of the cache chunk showing the data type, item count, and time range.
    /// </summary>
    /// <returns>A string representation of the cache chunk</returns>
    public override string ToString() => $"{typeof(T).FriendlyName()}[{Items.Count}] ({Range.S()})";
}
