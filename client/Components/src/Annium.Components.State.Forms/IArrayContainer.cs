using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

public interface IArrayContainer<T> : IValueTrackedState<List<T>>
    where T : notnull, new()
{
    IReadOnlyList<ITrackedState> Children { get; }

    IObjectContainer<TI> AtObject<TI>(Expression<Func<List<T>, TI>> ex)
        where TI : notnull, new();

    IArrayContainer<TI> AtArray<TI>(Expression<Func<List<T>, List<TI>>> ex)
        where TI : notnull, new();

    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<List<T>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<List<T>, TI>> ex);

    void Add(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}
