using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

internal class StateFactory : IStateFactory
{
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public StateFactory(IMapper mapper, ILogger logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public IAtomicContainer<T> CreateAtomic<T>(T defaultValue)
    {
        return new AtomicContainer<T>(defaultValue, _logger);
    }

    public IArrayContainer<T> CreateArray<T>(List<T> initialValue)
        where T : notnull, new()
    {
        return new ArrayContainer<T>(initialValue, this, _mapper, _logger);
    }

    public IMapContainer<TKey, TValue> CreateMap<TKey, TValue>(Dictionary<TKey, TValue> initialValue)
        where TKey : IEquatable<TKey>
        where TValue : notnull, new()
    {
        return new MapContainer<TKey, TValue>(initialValue.ToDictionary(), this, _mapper, _logger);
    }

    public IObjectContainer<T> CreateObject<T>(T initialValue)
        where T : notnull, new()
    {
        return new ObjectContainer<T>(initialValue, this, _logger);
    }
}
