using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NodaTime;

namespace Annium.Components.State.Forms;

public interface IMapContainer<TKey, TValue> : IValueTrackedState<Dictionary<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull, new()
{
    IReadOnlyCollection<TKey> Keys { get; }
    IArrayContainer<TI> At<TI>(Expression<Func<Dictionary<TKey, TValue>, List<TI>>> ex) where TI : notnull, new();
    IMapContainer<TK, TV> At<TK, TV>(Expression<Func<Dictionary<TKey, TValue>, Dictionary<TK, TV>>> ex) where TK : notnull where TV : notnull, new();
    IAtomicContainer<sbyte> At(Expression<Func<Dictionary<TKey, TValue>, sbyte>> ex);
    IAtomicContainer<short> At(Expression<Func<Dictionary<TKey, TValue>, short>> ex);
    IAtomicContainer<int> At(Expression<Func<Dictionary<TKey, TValue>, int>> ex);
    IAtomicContainer<long> At(Expression<Func<Dictionary<TKey, TValue>, long>> ex);
    IAtomicContainer<byte> At(Expression<Func<Dictionary<TKey, TValue>, byte>> ex);
    IAtomicContainer<ushort> At(Expression<Func<Dictionary<TKey, TValue>, ushort>> ex);
    IAtomicContainer<uint> At(Expression<Func<Dictionary<TKey, TValue>, uint>> ex);
    IAtomicContainer<ulong> At(Expression<Func<Dictionary<TKey, TValue>, ulong>> ex);
    IAtomicContainer<decimal> At(Expression<Func<Dictionary<TKey, TValue>, decimal>> ex);
    IAtomicContainer<float> At(Expression<Func<Dictionary<TKey, TValue>, float>> ex);
    IAtomicContainer<double> At(Expression<Func<Dictionary<TKey, TValue>, double>> ex);
    IAtomicContainer<string> At(Expression<Func<Dictionary<TKey, TValue>, string>> ex);
    IAtomicContainer<bool> At(Expression<Func<Dictionary<TKey, TValue>, bool>> ex);
    IAtomicContainer<DateTime> At(Expression<Func<Dictionary<TKey, TValue>, DateTime>> ex);
    IAtomicContainer<DateTimeOffset> At(Expression<Func<Dictionary<TKey, TValue>, DateTimeOffset>> ex);
    IAtomicContainer<Instant> At(Expression<Func<Dictionary<TKey, TValue>, Instant>> ex);
    IObjectContainer<TI> At<TI>(Expression<Func<Dictionary<TKey, TValue>, TI>> ex) where TI : notnull, new();
    void Add(TKey key, TValue item);
    void Remove(TKey key);
}