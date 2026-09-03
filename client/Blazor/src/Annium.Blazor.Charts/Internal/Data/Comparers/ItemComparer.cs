using System;
using System.Collections.Generic;

namespace Annium.Blazor.Charts.Internal.Data.Comparers;

/// <summary>
/// Provides comparison functionality for items using a custom comparison function
/// </summary>
/// <typeparam name="T">The type of items to compare</typeparam>
internal class ItemComparer<T> : IComparer<T>
{
    /// <summary>
    /// The comparison function used to compare two items
    /// </summary>
    private readonly Func<T, T, int> _compare;

    /// <summary>
    /// Initializes a new instance of the ItemComparer class
    /// </summary>
    /// <param name="compare">The function to use for comparing items</param>
    public ItemComparer(Func<T, T, int> compare)
    {
        _compare = compare;
    }

    /// <summary>
    /// Compares two items and returns an integer indicating their relative order
    /// </summary>
    /// <param name="x">The first item to compare</param>
    /// <param name="y">The second item to compare</param>
    /// <returns>A value less than zero if x is less than y, zero if they are equal, or greater than zero if x is greater than y</returns>
    /// <exception cref="ArgumentNullException">Thrown when either x or y is null</exception>
    public int Compare(T? x, T? y)
    {
        if (x is null || y is null)
            throw new ArgumentNullException($"Can't compare null values of {typeof(T).FriendlyName()}");

        return _compare(x, y);
    }
}
