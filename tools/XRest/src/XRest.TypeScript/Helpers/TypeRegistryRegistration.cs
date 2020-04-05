using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Helpers
{
    internal partial class TypeRegistry
    {
        private readonly TypeMap _views = new TypeMap();

        public IReadOnlyCollection<DefinedTypeView> Register(IReadOnlyCollection<Type> types)
        {
            var views = types.Select(Register).ToDictionary(x => x.Item1, x => x.Item2);

            foreach (var (type, view) in views.Where(x => x.Value is ClassView))
            {
                var classView = (ClassView) view;
                var properties = type.GetAllPublicProperties()
                    .Select(ResolveProperty)
                    .ToArray();

                classView.Configure(properties);
            }

            return views.Values;
        }

        private (Type, DefinedTypeView) Register(Type type)
        {
            if (TryResolve(type, out var view))
                return (type, view!);

            if (type.IsEnum)
                return (type, RegisterEnum(type));

            return (type, RegisterClass(type));
        }

        private DefinedTypeView RegisterClass(Type type)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                // TODO: register type arguments
                throw new ArgumentException($"Can't register generic type {type} as compiled generic type");

            var view = type.ContainsGenericParameters
                ? new ClassView(
                    $"{type.Name[..type.Name.IndexOf('`')]}T{type.GetTypeInfo().GenericTypeParameters.Length}",
                    type.GetTypeInfo().GenericTypeParameters.Select(x => new GenericParameterView(x.Name)).ToArray()
                )
                : new ClassView(type.Name);
            _views.Register(type, view);

            return view;
        }

        private DefinedTypeView RegisterEnum(Type type)
        {
            var values = Enum.GetNames(type)
                .Zip(Enum.GetValues(type).Cast<int>())
                .ToDictionary(x => x.First, x => x.Second);

            var view = new EnumView(type.Name, values);
            _views.Register(type, view);

            return view;
        }

        private TypePropertyView ResolveProperty(PropertyInfo property)
        {
            TypeView view;
            if (TryResolve(property.PropertyType, out var resolvedView))
                view = resolvedView!;
            else if (property.PropertyType.IsGenericParameter)
                view = new GenericParameterView(property.PropertyType.Name);
            else
                view = Resolve(property.PropertyType);

            return new TypePropertyView(property.Name, view, view.Name == BaseType.Nullable.Name);
        }
    }
}