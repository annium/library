using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NodaTime;

namespace Annium.Components.State.Forms;

public interface IObjectContainer<T> : IState<T>
    where T : notnull, new()
{
    IReadOnlyDictionary<string, IState> Children { get; }
    IArrayContainer<TI> At<TI>(Expression<Func<T, IEnumerable<TI>>> ex) where TI : notnull, new();
    IMapContainer<TK, TV> At<TK, TV>(Expression<Func<T, IEnumerable<KeyValuePair<TK, TV>>>> ex) where TK : notnull where TV : notnull, new();
    IAtomicContainer<sbyte> At(Expression<Func<T, sbyte>> ex);
    IAtomicContainer<short> At(Expression<Func<T, short>> ex);
    IAtomicContainer<int> At(Expression<Func<T, int>> ex);
    IAtomicContainer<long> At(Expression<Func<T, long>> ex);
    IAtomicContainer<byte> At(Expression<Func<T, byte>> ex);
    IAtomicContainer<ushort> At(Expression<Func<T, ushort>> ex);
    IAtomicContainer<uint> At(Expression<Func<T, uint>> ex);
    IAtomicContainer<ulong> At(Expression<Func<T, ulong>> ex);
    IAtomicContainer<decimal> At(Expression<Func<T, decimal>> ex);
    IAtomicContainer<float> At(Expression<Func<T, float>> ex);
    IAtomicContainer<double> At(Expression<Func<T, double>> ex);
    IAtomicContainer<string> At(Expression<Func<T, string>> ex);
    IAtomicContainer<bool> At(Expression<Func<T, bool>> ex);
    IAtomicContainer<DateTime> At(Expression<Func<T, DateTime>> ex);
    IAtomicContainer<DateTimeOffset> At(Expression<Func<T, DateTimeOffset>> ex);
    IAtomicContainer<Instant> At(Expression<Func<T, Instant>> ex);
    IObjectContainer<TI> At<TI>(Expression<Func<T, TI>> ex) where TI : notnull, new();
}