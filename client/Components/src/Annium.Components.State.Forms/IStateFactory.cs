using System;
using System.Collections.Generic;

namespace Annium.Components.State.Forms;

public interface IStateFactory
{
    IAtomicContainer<T> CreateAtomic<T>(T defaultValue);

    IArrayContainer<T> CreateArray<T>(List<T> initialValue)
        where T : notnull, new();

    IMapContainer<TKey, TValue> CreateMap<TKey, TValue>(Dictionary<TKey, TValue> initialValue)
        where TKey : IEquatable<TKey>
        where TValue : notnull, new();

    IObjectContainer<T> CreateObject<T>(T initialValue)
        where T : notnull, new();
}