using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Components.State.Forms
{
    public interface IStateFactory
    {
        IAtomicContainer<sbyte> Create(sbyte initialValue);
        IAtomicContainer<short> Create(short initialValue);
        IAtomicContainer<int> Create(int initialValue);
        IAtomicContainer<long> Create(long initialValue);
        IAtomicContainer<byte> Create(byte initialValue);
        IAtomicContainer<ushort> Create(ushort initialValue);
        IAtomicContainer<uint> Create(uint initialValue);
        IAtomicContainer<ulong> Create(ulong initialValue);
        IAtomicContainer<decimal> Create(decimal initialValue);
        IAtomicContainer<float> Create(float initialValue);
        IAtomicContainer<double> Create(double initialValue);
        IAtomicContainer<string> Create(string initialValue);
        IAtomicContainer<bool> Create(bool initialValue);
        IAtomicContainer<Guid> Create(Guid initialValue);
        IAtomicContainer<DateTime> Create(DateTime initialValue);
        IAtomicContainer<DateTimeOffset> Create(DateTimeOffset initialValue);
        IAtomicContainer<Instant> Create(Instant initialValue);
        IMapContainer<TKey, TValue> Create<TKey, TValue>(IDictionary<TKey, TValue> initialValue) where TKey : notnull where TValue : notnull, new();
        IMapContainer<TKey, TValue> Create<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> initialValue) where TKey : notnull where TValue : notnull, new();
        IMapContainer<TKey, TValue> Create<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> initialValue) where TKey : notnull where TValue : notnull, new();
        IArrayContainer<T> Create<T>(IEnumerable<T> initialValue) where T : notnull, new();
        IObjectContainer<T> Create<T>(T initialValue) where T : notnull, new();
    }
}