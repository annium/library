using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;

namespace XRest.Core.Views.Profiles
{
    public static class TypeViewDeserializationProfileExtensions
    {
        public static Profile ConfigureForTypeViewDeserialization(this Profile profile, Assembly assembly)
        {
            var types = TypeManager.GetInstance(assembly).Types;

            profile.Map<TypeView?, Type?>(x => ResolveType(types, x));

            return profile;
        }

        private static Type? ResolveType(IReadOnlyCollection<Type> types, TypeView? view)
        {
            if (view is null)
                return null;

            var type = Resolve(view.FullName);

            if (!type.IsGenericType)
                return type;

            var arguments = view.GenericArguments.Select(x => ResolveType(types, x)!).ToArray();

            return type.MakeGenericType(arguments);

            Type Resolve(string fullName) => types.Single(x => x.FullName == fullName);
        }
    }
}