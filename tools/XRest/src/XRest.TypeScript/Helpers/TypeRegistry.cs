using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Core.Reflection;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Helpers
{
    internal class TypeRegistry
    {
        private readonly TypeMap _views = new TypeMap();

        public IReadOnlyCollection<TypeView> Register(IReadOnlyCollection<Type> types) =>
            types.Select(Register).ToArray();

        public TypeView Register(Type type)
        {
            if (TryResolve(type, out var view))
                return view!;

            view = type.ContainsGenericParameters
                ? new TypeView(
                    $"{type.Name[..type.Name.IndexOf('`')]}T{type.GetTypeInfo().GenericTypeParameters.Length}",
                    type.GetTypeInfo().GenericTypeParameters.Select(x => new TypeView(x.Name, true)).ToArray()
                )
                : new TypeView(type.Name, false);
            _views.Register(type, view);

            var properties = type.GetAllPublicProperties()
                .Select(ProcessProperty)
                .ToArray();

            view.Configure(properties);

            return view;
        }

        public TypeView Resolve(Type type)
        {
            if (type.IsGenericType)
                return ResolveGenericType(type);

            // non-generic dictionary
            if (type.IsDictionary())
                return BaseType.Object;

            // non-generic array
            if (type.IsArray())
                return BaseType.Array.MakeGenericType(BaseType.Object);

            return ResolveInternal(type);
        }

        private TypeView ResolveGenericType(Type type)
        {
            var definition = type.GetGenericTypeDefinition();

            if (type.IsGenericTypeDefinition)
                return ResolveInternal(type);

            // Array
            if (type.IsArray)
                return BaseType.Array.MakeGenericType(Resolve(type.GetElementType()!));

            // Task<>
            if (definition == typeof(Task<>))
                return Resolve(type.GetGenericArguments().Single());

            // IDictionary<> | IReadOnlyDictionary
            if (type.IsDictionary())
            {
                var keyValueTypeParams = type.GetTargetImplementation(typeof(IEnumerable<>))!.GetGenericArguments().Single().GetGenericArguments();

                return BaseType.Record.MakeGenericType(
                    Resolve(keyValueTypeParams[0]),
                    Resolve(keyValueTypeParams[1])
                );
            }

            // IEnumerable<>
            if (type.IsArray())
            {
                var elementType = type.GetTargetImplementation(typeof(IEnumerable<>))!.GetGenericArguments().Single();

                return BaseType.Array.MakeGenericType(Resolve(elementType));
            }

            var view = Resolve(definition);
            var arguments = type.GetGenericArguments().Select(Resolve).ToArray();

            return view.MakeGenericType(arguments);
        }

        private TypePropertyView ProcessProperty(PropertyInfo property)
        {
            if (!TryResolve(property.PropertyType, out var view))
                view = property.PropertyType.IsGenericParameter
                    ? new TypeView(property.PropertyType.Name, true)
                    : Register(property.PropertyType);

            return new TypePropertyView(property.Name, view!, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolve(Type type, out TypeView? view) => _views.TryGet(type, out view) || Types.BuiltIn.TryGet(type, out view);

        private TypeView ResolveInternal(Type type)
        {
            if (TryResolve(type, out var view))
                return view!;

            throw new InvalidOperationException($"Type with name '{type.Name}' not found in registry");
        }
    }
}