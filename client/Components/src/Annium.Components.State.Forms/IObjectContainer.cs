using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Components.State.Forms;

public interface IObjectContainer<T> : IValueTrackedState<T>
    where T : notnull, new()
{
    IReadOnlyDictionary<string, ITrackedState> Children { get; }

    IObjectContainer<TI> AtObject<TI>(Expression<Func<T, TI>> ex)
        where TI : notnull, new();

    IArrayContainer<TI> AtArray<TI>(Expression<Func<T, List<TI>>> ex)
        where TI : notnull, new();

    IMapContainer<TK, TV> AtMap<TK, TV>(Expression<Func<T, Dictionary<TK, TV>>> ex)
        where TK : notnull
        where TV : notnull, new();

    IAtomicContainer<TI> AtAtomic<TI>(Expression<Func<T, TI>> ex);
}
