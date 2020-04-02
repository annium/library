using System.Collections.Generic;

namespace XRest.TypeScript.Models
{
    internal class TypeView
    {
        public string Name { get; }
        public bool HasGenericArguments => GenericArgumentsCount > 0;
        public int GenericArgumentsCount { get; }
        public TypeView? BaseType { get; }
        public IReadOnlyCollection<TypeView> Interfaces { get; }
        public IReadOnlyCollection<TypePropertyView> Properties { get; }

        public TypeView(
            string name,
            int genericArgumentsCount,
            TypeView? baseType,
            IReadOnlyCollection<TypeView> interfaces,
            IReadOnlyCollection<TypePropertyView> properties
        )
        {
            Name = name;
            GenericArgumentsCount = genericArgumentsCount;
            BaseType = baseType;
            Interfaces = interfaces;
            Properties = properties;
        }
    }
}