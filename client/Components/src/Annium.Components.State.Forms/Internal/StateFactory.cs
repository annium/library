using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Internal implementation of IStateFactory that creates various types of state containers.
/// Uses dependency injection to provide mapper and logger services to created containers.
/// </summary>
internal class StateFactory : IStateFactory
{
    /// <summary>
    /// The mapper service used for object mapping operations in created containers.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// The logger service provided to created containers for logging operations.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the StateFactory class with the specified dependencies.
    /// </summary>
    /// <param name="mapper">The mapper service for object mapping operations</param>
    /// <param name="logger">The logger service for logging operations</param>
    public StateFactory(IMapper mapper, ILogger logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new atomic container for managing state of a single value.
    /// </summary>
    /// <typeparam name="T">The type of value to be contained</typeparam>
    /// <param name="defaultValue">The initial value for the container</param>
    /// <returns>A new atomic container initialized with the default value</returns>
    public IAtomicContainer<T> CreateAtomic<T>(T defaultValue)
    {
        return new AtomicContainer<T>(defaultValue, _logger);
    }

    /// <summary>
    /// Creates a new array container for managing state of a list of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <param name="initialValue">The initial list of items for the container</param>
    /// <returns>A new array container initialized with the initial list</returns>
    public IArrayContainer<T> CreateArray<T>(List<T> initialValue)
        where T : notnull, new()
    {
        return new ArrayContainer<T>(initialValue, this, _mapper, _logger);
    }

    /// <summary>
    /// Creates a new map container for managing state of a dictionary of key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
    /// <param name="initialValue">The initial dictionary for the container</param>
    /// <returns>A new map container initialized with the initial dictionary</returns>
    public IMapContainer<TKey, TValue> CreateMap<TKey, TValue>(Dictionary<TKey, TValue> initialValue)
        where TKey : IEquatable<TKey>
        where TValue : notnull, new()
    {
        return new MapContainer<TKey, TValue>(initialValue.ToDictionary(), this, _mapper, _logger);
    }

    /// <summary>
    /// Creates a new object container for managing state of a complex object with multiple properties.
    /// </summary>
    /// <typeparam name="T">The type of object to be contained</typeparam>
    /// <param name="initialValue">The initial object value for the container</param>
    /// <returns>A new object container initialized with the initial object</returns>
    public IObjectContainer<T> CreateObject<T>(T initialValue)
        where T : notnull, new()
    {
        return new ObjectContainer<T>(initialValue, this, _logger);
    }
}
