using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Helpers
{
    internal class TypeRegistry
    {
        private readonly IDictionary<Type, TypeView> _views = new Dictionary<Type, TypeView>();

        public IReadOnlyCollection<TypeView> Register(IReadOnlyCollection<Type> types) =>
            types.Select(Register).ToArray();

        public TypeView Register(Type type)
        {
            var (view, isAdded) = ResolveInternal(type);
            if (!isAdded)
                return view;

            // var baseTypeView = type.BaseType != null && type.BaseType != typeof(object)
            //     ? Register(type.BaseType)
            //     : null;
            // var interfaces = type.GetInterfaces().Select(Register).ToArray();
            var properties = ProcessorHelper.GetProperties(type)
                .Select(ProcessProperty)
                .ToArray();

            view.Configure( /*baseTypeView, interfaces, */properties);

            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeView Resolve(Type type)
        {
            if (_views.TryGetValue(type, out var typeView))
                return typeView;

            throw new InvalidOperationException($"Type with name '{type.Name}' not found in registry");
        }

        private (TypeView, bool) ResolveInternal(Type type)
        {
            if (_views.TryGetValue(type, out var typeView))
                return (typeView, false);

            if (TypeMap.BaseTypes.ContainsKey(type))
                return (TypeMap.BaseTypes[type], false);

            // resolve unique name
            if (_views.Keys.Any(x => x.Name == type.Name))
                throw new InvalidOperationException($"Type with name '{type.Name}' is already registered in registry");

            typeView = type.ContainsGenericParameters
                ? new TypeView(
                    $"{type.Name[..type.Name.IndexOf('`')]}T{type.GetTypeInfo().GenericTypeParameters.Length}",
                    type.GetTypeInfo().GenericTypeParameters.Select(x => new TypeView(x.Name)).ToArray()
                )
                : new TypeView(type.Name);

            return (_views[type] = typeView, true);
        }

        private TypePropertyView ProcessProperty(PropertyInfo property)
        {
            var propertyType = TypeMap.BaseTypes.FirstOrDefault(x => x.Key.FullName == property.PropertyType.FullName).Value;
            if (propertyType is null)
                propertyType = property.PropertyType.IsGenericParameter
                    ? new TypeView(property.PropertyType.Name)
                    : Register(property.PropertyType);

            return new TypePropertyView(
                property.Name,
                propertyType,
                false
            );
        }
    }
}