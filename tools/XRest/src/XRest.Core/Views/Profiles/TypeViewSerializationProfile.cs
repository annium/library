using System;
using System.Linq;
using Annium.Core.Mapper;

namespace XRest.Core.Views.Profiles
{
    public class TypeViewSerializationProfile : Profile
    {
        public TypeViewSerializationProfile()
        {
            Map<Type, TypeView>(x => BuildTypeView(x));
        }

        private TypeView BuildTypeView(Type type)
        {
            if (!type.IsGenericType)
                return new TypeView { FullName = type.FullName! };

            var definition = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments().Select(BuildTypeView).ToArray();

            return new TypeView
            {
                FullName = definition.FullName!,
                GenericArguments = arguments,
            };
        }
    }
}