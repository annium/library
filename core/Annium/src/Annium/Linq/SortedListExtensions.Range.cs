using System;
using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

/// <summary>Provides extension methods for working with ranges in sorted lists.</summary>
public static class SortedListRangeExtensions
{
    /// <summary>
    /// Merges a range of key-value pairs into the sorted list, returning the merged list and leaving
    /// <paramref name="source"/> untouched.
    /// </summary>
    /// <remarks>
    /// A new list rather than an in-place add, because in place cannot be done cheaply.
    /// <see cref="SortedList{TKey,TValue}"/> keeps its keys in an array, so a single insert below the
    /// last key shifts everything after it; adding m items to a list of n one by one costs O(n·m), and
    /// the enumeration order of an <see cref="IReadOnlyDictionary{TKey,TValue}"/> is arbitrary, so the
    /// caller cannot dodge it by handing the keys over in order. Merging two ordered sequences into a
    /// fresh list appends throughout — every insertion point is the end — and costs O((n+m)·log(n+m)).
    /// The difference is not academic: 700k candles merged into 750k took 41 seconds one at a time.
    /// </remarks>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to merge into.</param>
    /// <param name="range">The range of key-value pairs to merge in.</param>
    /// <returns>A new sorted list holding both, or <paramref name="source"/> itself when nothing is added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a key from <paramref name="range"/> already exists in <paramref name="source"/>.</exception>
    public static SortedList<TKey, TValue> Merge<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        if (range.Count == 0)
            return source;

        var comparer = source.Comparer;

        var added = new KeyValuePair<TKey, TValue>[range.Count];
        var index = 0;
        foreach (var pair in range)
            added[index++] = pair;
        Array.Sort(added, (a, b) => comparer.Compare(a.Key, b.Key));

        var keys = source.Keys;
        var values = source.Values;
        var result = new SortedList<TKey, TValue>(source.Count + range.Count, comparer);

        int i = 0,
            j = 0;
        while (i < keys.Count && j < added.Length)
        {
            var order = comparer.Compare(keys[i], added[j].Key);
            if (order == 0)
                throw new InvalidOperationException($"Trying to add duplicate key {keys[i]}");

            if (order < 0)
            {
                result.Add(keys[i], values[i]);
                i++;
            }
            else
            {
                result.Add(added[j].Key, added[j].Value);
                j++;
            }
        }

        for (; i < keys.Count; i++)
            result.Add(keys[i], values[i]);

        for (; j < added.Length; j++)
            result.Add(added[j].Key, added[j].Value);

        return result;
    }

    /// <summary>Sets a range of key-value pairs in the sorted list, replacing any existing values for duplicate keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to set the range in.</param>
    /// <param name="range">The range of key-value pairs to set.</param>
    public static void SetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        foreach (var (key, value) in range)
            source[key] = value;
    }

    /// <summary>Gets a span of the sorted list between the specified start and end keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to get the range from.</param>
    /// <param name="start">The start key of the range.</param>
    /// <param name="end">The end key of the range.</param>
    /// <returns>A span of the sorted list between the specified keys, or null if the keys are not found.</returns>
    public static ISortedListSpan<TKey, TValue>? GetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end
    )
        where TKey : notnull
    {
        var startIndex = source.IndexOfKey(start);
        var endIndex = source.IndexOfKey(end);

        if (startIndex < 0 || endIndex < 0)
            return null;

        return new SortedListSpan<TKey, TValue>(source, startIndex, endIndex - startIndex + 1);
    }
}
