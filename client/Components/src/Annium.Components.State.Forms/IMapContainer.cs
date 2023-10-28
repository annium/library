using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

public interface IMapContainer<TKey, TValue> : IValueTrackedState<Dictionary<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull, new()
{
    IReadOnlyCollection<TKey> Keys { get; }

    IObjectContainer<TI> AtObject<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex)
        where TI : notnull, new();

    IArrayContainer<TI> AtArray<TI>(Expression<Func<Dictionary<TKey, TValue>, List<TI>>> ex)
        where TI : notnull, new();

    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<Dictionary<TKey, TValue>, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex);

    void Add(TKey key, TValue item);
    void Remove(TKey key);
}
