using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Data.Models;

namespace XRest.TypeScript.Models
{
    internal class TypeView : Equatable<TypeView>
    {
        public string Name { get; }
        public bool IsGenericType { get; }
        public bool IsGenericTypeDefinition { get; }
        public bool IsGenericTypeParameter { get; }
        public IReadOnlyCollection<TypeView> GenericParameters { get; } = Array.Empty<TypeView>();

        public IReadOnlyCollection<TypePropertyView> Properties { get; private set; } = Array.Empty<TypePropertyView>();
        private bool _isConfigured;

        public TypeView(
            string name,
            IReadOnlyCollection<TypeView> genericParameters
        ) : this(name, false)
        {
            if (genericParameters.Count == 0)
                throw new ArgumentException("Generic parameters count must be greater than 0");

            var genericParametersCount = genericParameters.Count(x => x.IsGenericTypeParameter);
            if (0 < genericParametersCount && genericParametersCount < genericParameters.Count)
                throw new ArgumentException("Generic parameters must not be mixed with generic arguments");

            GenericParameters = genericParameters;
            IsGenericType = true;
            IsGenericTypeDefinition = genericParametersCount > 0;
        }

        public TypeView(
            string name,
            bool isGenericTypeParameter
        )
        {
            Name = name;
            IsGenericTypeParameter = isGenericTypeParameter;
        }

        public void Configure(
            IReadOnlyCollection<TypePropertyView> properties
        )
        {
            if (_isConfigured)
                throw new InvalidOperationException("TypeView is already configured");

            Properties = properties;
            _isConfigured = true;
        }

        public TypeView MakeGenericType(
            params TypeView[] arguments
        )
        {
            if (!IsGenericTypeDefinition)
                throw new InvalidOperationException($"{this} is not generic type definition");

            if (arguments.Length != GenericParameters.Count || arguments.Any(x => x.IsGenericTypeParameter))
                throw new InvalidOperationException($"Expected {GenericParameters.Count} generic arguments");

            return new TypeView(Name, arguments);
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public override string ToString()
        {
            if (!IsGenericType)
                return Name;

            return $"{Name}<{string.Join(", ", GenericParameters)}>";
        }
    }
}