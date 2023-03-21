using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;

namespace Annium.Components.State.Forms.Internal;

internal class StateFactory : IStateFactory
{
    private readonly IMapper _mapper;

    public StateFactory(IMapper mapper)
    {
        _mapper = mapper;
    }

    public IAtomicContainer<T> CreateAtomic<T>(T defaultValue)
        where T : IEquatable<T>
        => new AtomicContainer<T>(defaultValue);

    public IArrayContainer<T> CreateArray<T>(List<T> initialValue)
        where T : notnull, new()
        => new ArrayContainer<T>(initialValue, this, _mapper);

    public IMapContainer<TKey, TValue> CreateMap<TKey, TValue>(Dictionary<TKey, TValue> initialValue)
        where TKey : IEquatable<TKey>
        where TValue : notnull, new()
        => new MapContainer<TKey, TValue>(initialValue.ToDictionary(x => x.Key, x => x.Value), this, _mapper);

    public IObjectContainer<T> CreateObject<T>(T initialValue)
        where T : notnull, new()
        => new ObjectContainer<T>(initialValue, this);
}