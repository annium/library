using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace XRest.TypeScript.Models
{
    internal class TypeView : Equatable<TypeView>
    {
        public string Name { get; }
        public IReadOnlyCollection<TypeView> GenericParameters { get; } = Array.Empty<TypeView>();

        // public TypeView? BaseType { get; private set; }
        // public IReadOnlyCollection<TypeView> Interfaces { get; private set; } = Array.Empty<TypeView>();
        public IReadOnlyCollection<TypePropertyView> Properties { get; private set; } = Array.Empty<TypePropertyView>();
        private bool _isConfigured;

        public TypeView(
            string name,
            IReadOnlyCollection<TypeView> genericParameters
        ) : this(name)
        {
            GenericParameters = genericParameters;
        }

        public TypeView(
            string name
        )
        {
            Name = name;
        }

        public void Configure(
            // TypeView? baseType,
            // IReadOnlyCollection<TypeView> interfaces,
            IReadOnlyCollection<TypePropertyView> properties
        )
        {
            if (_isConfigured)
                throw new InvalidOperationException("TypeView is already configured");

            // BaseType = baseType;
            // Interfaces = interfaces;
            Properties = properties;
            _isConfigured = true;
        }

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name;
    }
}