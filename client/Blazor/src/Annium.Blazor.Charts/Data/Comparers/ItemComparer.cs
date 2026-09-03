using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Annium.Blazor.Charts.Internal.Data.Comparers;

namespace Annium.Blazor.Charts.Data.Comparers;

/// <summary>
/// Provides factory methods for creating and caching type-specific item comparers
/// </summary>
public static class ItemComparer
{
    /// <summary>
    /// Cache of type-specific comparers to avoid recreation
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> _comparers = new();

    /// <summary>
    /// Creates or retrieves a cached comparer for the specified type
    /// </summary>
    /// <typeparam name="T">The type of items to compare</typeparam>
    /// <param name="compare">The comparison function to use</param>
    /// <returns>An IComparer instance for the specified type</returns>
    public static IComparer<T> For<T>(Func<T, T, int> compare) =>
        (IComparer<T>)_comparers.GetOrAdd(typeof(T), static (_, comp) => new ItemComparer<T>(comp), compare);
}
