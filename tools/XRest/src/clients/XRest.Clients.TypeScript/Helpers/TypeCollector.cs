using System;
using System.Collections.Generic;
using System.Linq;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Helpers;

internal class TypeCollector
{
    public IReadOnlyCollection<Type> CollectedTypes => _collectedTypes;
    private readonly HashSet<Type> _collectedTypes = new HashSet<Type>();

    public void CollectTypes(Type? type)
    {
        if (type is null)
            return;

        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        var isCollected = !KnownTypes.BuiltIn.Contains(definition) && !definition.IsSkipped() && _collectedTypes.Add(definition);

        if (type.IsGenericType)
            foreach (var typeArgument in type.GenericTypeArguments)
                CollectTypes(typeArgument);

        if (isCollected)
            CollectPropertiesTypes(type);
    }

    private void CollectPropertiesTypes(Type type)
    {
        var propertyTypes = type.GetAllPublicProperties()
            .Select(x => x.PropertyType)
            .ToArray();

        foreach (var propertyType in propertyTypes)
            CollectTypes(propertyType);
    }
}