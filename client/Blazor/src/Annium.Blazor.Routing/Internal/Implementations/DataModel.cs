using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Net.Base;

namespace Annium.Blazor.Routing.Internal.Implementations;

internal class DataModel : IDataModel
{
    public static IReadOnlyCollection<PropertyInfo> ResolveProperties<T>() => typeof(T)
        .GetProperties()
        .Where(x => x.CanWrite)
        .ToArray();

    public static DataModel Create<T>(IReadOnlyCollection<PropertyInfo> properties, IMapper mapper)
    {
        foreach (var property in properties)
            if (property.DeclaringType != typeof(T))
                throw new ArgumentException($"Property '{property}' is declared not in type '{typeof(T)}'");

        return new DataModel(typeof(T), properties, mapper);
    }

    private readonly Type _type;
    private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;
    private readonly IMapper _mapper;

    private DataModel(
        Type type,
        IEnumerable<PropertyInfo> properties,
        IMapper mapper
    )
    {
        _type = type;
        _mapper = mapper;
        _properties = properties.ToPropertiesDictionary();
    }

    public IReadOnlyDictionary<string, object?> ToParams(object data)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (data is null)
            return new Dictionary<string, object?>();

        var dataType = data.GetType();
        if (dataType != _type && !data.GetType().IsDerivedFrom(_type))
            throw new ArgumentException($"Data type '{data.GetType().FriendlyName()}' is not derived from '{_type.FriendlyName()}'");

        var values = new Dictionary<string, object?>();
        foreach (var (key, property) in _properties)
            values[key] = property.GetValue(data);

        return values;
    }

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

            query[key] = property.PropertyType.IsEnumerable() ? _mapper.Map<string[]>(value) : _mapper.Map<string>(value);
        }

        return query;
    }
}