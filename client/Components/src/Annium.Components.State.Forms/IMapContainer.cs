using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

/// <summary>
/// Represents a container for dictionary/map values with nested container access capabilities.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public interface IMapContainer<TKey, TValue> : IValueTrackedState<Dictionary<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull, new()
{
    /// <summary>
    /// Gets the keys present in the dictionary.
    /// </summary>
    IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Gets an object container for a nested object property.
    /// </summary>
    /// <typeparam name="TI">The type of the nested object.</typeparam>
    /// <param name="ex">Expression to access the nested object.</param>
    /// <returns>An object container for the specified property.</returns>
    IObjectContainer<TI> AtObject<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Gets an array container for a nested array property.
    /// </summary>
    /// <typeparam name="TI">The type of elements in the nested array.</typeparam>
    /// <param name="ex">Expression to access the nested array.</param>
    /// <returns>An array container for the specified property.</returns>
    IArrayContainer<TI> AtArray<TI>(Expression<Func<Dictionary<TKey, TValue>, List<TI>>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Gets a map container for a nested dictionary property.
    /// </summary>
    /// <typeparam name="TK">The type of keys in the nested dictionary.</typeparam>
    /// <typeparam name="TV">The type of values in the nested dictionary.</typeparam>
    /// <param name="ex">Expression to access the nested dictionary.</param>
    /// <returns>A map container for the specified property.</returns>
    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<Dictionary<TKey, TValue>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    /// <summary>
    /// Gets an atomic container for a nested atomic property.
    /// </summary>
    /// <typeparam name="TI">The type of the nested atomic value.</typeparam>
    /// <param name="ex">Expression to access the nested atomic value.</param>
    /// <returns>An atomic container for the specified property.</returns>
    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex);

    /// <summary>
    /// Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="item">The value to add.</param>
    void Add(TKey key, TValue item);

    /// <summary>
    /// Removes the entry with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the entry to remove.</param>
    void Remove(TKey key);
}
