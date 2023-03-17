using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NodaTime;

namespace Annium.Components.State.Forms;

public interface IArrayContainer<T> : IValueTrackedState<List<T>>
    where T : notnull, new()
{
    IReadOnlyList<ITrackedState> Children { get; }
    IArrayContainer<TI> At<TI>(Expression<Func<List<T>, List<TI>>> ex) where TI : notnull, new();
    IMapContainer<TK, TV> At<TK, TV>(Expression<Func<List<T>, Dictionary<TK, TV>>> ex) where TK : notnull where TV : notnull, new();
    IAtomicContainer<sbyte> At(Expression<Func<List<T>, sbyte>> ex);
    IAtomicContainer<short> At(Expression<Func<List<T>, short>> ex);
    IAtomicContainer<int> At(Expression<Func<List<T>, int>> ex);
    IAtomicContainer<long> At(Expression<Func<List<T>, long>> ex);
    IAtomicContainer<byte> At(Expression<Func<List<T>, byte>> ex);
    IAtomicContainer<ushort> At(Expression<Func<List<T>, ushort>> ex);
    IAtomicContainer<uint> At(Expression<Func<List<T>, uint>> ex);
    IAtomicContainer<ulong> At(Expression<Func<List<T>, ulong>> ex);
    IAtomicContainer<decimal> At(Expression<Func<List<T>, decimal>> ex);
    IAtomicContainer<float> At(Expression<Func<List<T>, float>> ex);
    IAtomicContainer<double> At(Expression<Func<List<T>, double>> ex);
    IAtomicContainer<string> At(Expression<Func<List<T>, string>> ex);
    IAtomicContainer<bool> At(Expression<Func<List<T>, bool>> ex);
    IAtomicContainer<DateTime> At(Expression<Func<List<T>, DateTime>> ex);
    IAtomicContainer<DateTimeOffset> At(Expression<Func<List<T>, DateTimeOffset>> ex);
    IAtomicContainer<Instant> At(Expression<Func<List<T>, Instant>> ex);
    IObjectContainer<TI> At<TI>(Expression<Func<List<T>, TI>> ex) where TI : notnull, new();
    void Add(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}