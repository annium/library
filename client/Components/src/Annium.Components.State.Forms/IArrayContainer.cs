using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

/// <summary>
/// Represents a container for array/list values with nested container access capabilities.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public interface IArrayContainer<T> : IValueTrackedState<List<T>>
    where T : notnull, new()
{
    /// <summary>
    /// Gets the child tracked states for array elements.
    /// </summary>
    IReadOnlyList<ITrackedState> Children { get; }

    /// <summary>
    /// Gets an object container for a nested object property.
    /// </summary>
    /// <typeparam name="TI">The type of the nested object.</typeparam>
    /// <param name="ex">Expression to access the nested object.</param>
    /// <returns>An object container for the specified property.</returns>
    IObjectContainer<TI> AtObject<TI>(Expression<Func<List<T>, TI>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Gets an array container for a nested array property.
    /// </summary>
    /// <typeparam name="TI">The type of elements in the nested array.</typeparam>
    /// <param name="ex">Expression to access the nested array.</param>
    /// <returns>An array container for the specified property.</returns>
    IArrayContainer<TI> AtArray<TI>(Expression<Func<List<T>, List<TI>>> ex)
        where TI : notnull, new();

    /// <summary>
    /// Gets a map container for a nested dictionary property.
    /// </summary>
    /// <typeparam name="TK">The type of keys in the nested dictionary.</typeparam>
    /// <typeparam name="TV">The type of values in the nested dictionary.</typeparam>
    /// <param name="ex">Expression to access the nested dictionary.</param>
    /// <returns>A map container for the specified property.</returns>
    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<List<T>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    /// <summary>
    /// Gets an atomic container for a nested atomic property.
    /// </summary>
    /// <typeparam name="TI">The type of the nested atomic value.</typeparam>
    /// <param name="ex">Expression to access the nested atomic value.</param>
    /// <returns>An atomic container for the specified property.</returns>
    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<List<T>, TI>> ex);

    /// <summary>
    /// Adds an item to the end of the array.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(T item);

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    /// <param name="index">The index to insert at.</param>
    /// <param name="item">The item to insert.</param>
    void Insert(int index, T item);

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    void RemoveAt(int index);
}
