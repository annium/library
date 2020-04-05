using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Core.Reflection;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Helpers
{
    internal partial class TypeRegistry
    {
        public DefinedTypeView Resolve(Type type)
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

        private DefinedTypeView ResolveGenericType(Type type)
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

            var view = (ClassView) Resolve(definition);
            var arguments = type.GetGenericArguments().Select(Resolve).ToArray();

            return view.MakeGenericType(arguments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolve(Type type, out DefinedTypeView? view) => _views.TryGet(type, out view) || KnownTypes.BuiltIn.TryGet(type, out view);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DefinedTypeView ResolveInternal(Type type)
        {
            if (TryResolve(type, out var view))
                return view!;

            throw new InvalidOperationException($"Type with name '{type.Name}' not found in registry");
        }
    }
}