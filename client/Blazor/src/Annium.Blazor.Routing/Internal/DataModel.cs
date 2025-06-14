using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Net.Base;
using Annium.Reflection;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides data model functionality for converting between objects, parameters, and URI queries in routing.
/// </summary>
internal class DataModel : IDataModel
{
    /// <summary>
    /// Resolves all writable properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve properties for.</typeparam>
    /// <returns>A collection of writable property information.</returns>
    public static IReadOnlyCollection<PropertyInfo> ResolveProperties<T>() =>
        typeof(T).GetProperties().Where(x => x.CanWrite).ToArray();

    /// <summary>
    /// Creates a new DataModel instance for the specified type with the given properties and mapper.
    /// </summary>
    /// <typeparam name="T">The type this data model represents.</typeparam>
    /// <param name="properties">The properties to include in the data model.</param>
    /// <param name="mapper">The mapper to use for type conversions.</param>
    /// <returns>A new DataModel instance.</returns>
    public static DataModel Create<T>(IReadOnlyCollection<PropertyInfo> properties, IMapper mapper)
    {
        foreach (var property in properties)
            if (property.DeclaringType != typeof(T))
                throw new ArgumentException($"Property '{property}' is declared not in type '{typeof(T)}'");

        return new DataModel(typeof(T), properties, mapper);
    }

    /// <summary>
    /// The type this data model represents.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// Dictionary of property names (camelCase) to PropertyInfo objects.
    /// </summary>
    private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;

    /// <summary>
    /// The mapper used for type conversions.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the DataModel class.
    /// </summary>
    /// <param name="type">The type this data model represents.</param>
    /// <param name="properties">The properties to include in the data model.</param>
    /// <param name="mapper">The mapper to use for type conversions.</param>
    private DataModel(Type type, IEnumerable<PropertyInfo> properties, IMapper mapper)
    {
        _type = type;
        _mapper = mapper;
        _properties = properties.ToPropertiesDictionary();
    }

    /// <summary>
    /// Converts an object instance to a dictionary of parameter names and values.
    /// </summary>
    /// <param name="data">The object to convert to parameters.</param>
    /// <returns>A dictionary containing parameter names and their corresponding values.</returns>
    public IReadOnlyDictionary<string, object?> ToParams(object data)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (data is null)
            return new Dictionary<string, object?>();

        var dataType = data.GetType();
        if (dataType != _type && !data.GetType().IsDerivedFrom(_type))
            throw new ArgumentException(
                $"Data type '{data.GetType().FriendlyName()}' is not derived from '{_type.FriendlyName()}'"
            );

        var values = new Dictionary<string, object?>();
        foreach (var (key, property) in _properties)
            values[key] = property.GetValue(data);

        return values;
    }

    /// <summary>
    /// Converts a dictionary of parameters to an object instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create and populate.</typeparam>
    /// <param name="parameters">The parameters to use for populating the object.</param>
    /// <returns>A new instance of type T populated with the parameter values.</returns>
    public T ToData<T>(IReadOnlyDictionary<string, object?> parameters)
        where T : new()
    {
        var data = new T();

        foreach (var (name, value) in parameters)
        {
            if (!_properties.TryGetValue(name, out var property))
                continue;

            property.SetValue(data, value);
        }

        return data;
    }

    /// <summary>
    /// Converts a URI query to a dictionary of parameter names and values.
    /// </summary>
    /// <param name="query">The URI query to convert.</param>
    /// <returns>A dictionary containing parameter names and their corresponding values.</returns>
    public IReadOnlyDictionary<string, object?> ToParams(UriQuery query)
    {
        var parameters = new Dictionary<string, object?>();

        foreach (var (name, value) in query)
        {
            if (!_properties.TryGetValue(name, out var property))
                continue;

            var type = property.PropertyType;
            parameters[name] = _mapper.Map(type.IsEnumerable() ? value.ToArray() : value.FirstOrDefault(), type);
        }

        return parameters;
    }

    /// <summary>
    /// Converts a dictionary of parameters to a URI query.
    /// </summary>
    /// <param name="parameters">The parameters to convert to a URI query.</param>
    /// <returns>A URI query containing the parameter values.</returns>
    public UriQuery ToQuery(IReadOnlyDictionary<string, object?> parameters)
    {
        var query = UriQuery.New();

        foreach (var (key, value) in parameters)
        {
            // if parameter not defined in properties - skip
            if (!_properties.TryGetValue(key, out var property))
                continue;

            if (value is null || value.Equals(value.GetType().DefaultValue()))
                continue;

            query[key] = property.PropertyType.IsEnumerable()
                ? _mapper.Map<string[]>(value)
                : _mapper.Map<string>(value);
        }

        return query;
    }
}
