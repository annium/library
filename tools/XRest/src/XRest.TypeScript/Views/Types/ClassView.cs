using System;
using System.Collections.Generic;
using System.Linq;

namespace XRest.TypeScript.Views.Types
{
    internal class ClassView : DefinedTypeView
    {
        public bool IsGenericType { get; }
        public bool IsGenericTypeDefinition { get; }
        public IReadOnlyCollection<DefinedTypeView> GenericArguments { get; } = Array.Empty<DefinedTypeView>();
        public IReadOnlyCollection<GenericParameterView> GenericParameters { get; } = Array.Empty<GenericParameterView>();
        public IReadOnlyCollection<TypePropertyView> Properties { get; private set; } = Array.Empty<TypePropertyView>();
        private bool _isConfigured;

        public ClassView(
            string name,
            IReadOnlyCollection<DefinedTypeView> genericArguments
        ) : this(name)
        {
            if (genericArguments.Count == 0)
                throw new ArgumentException("Generic arguments count must be greater than 0");

            GenericArguments = genericArguments;
            IsGenericType = true;
        }

        public ClassView(
            string name,
            IReadOnlyCollection<GenericParameterView> genericParameters
        ) : this(name)
        {
            if (genericParameters.Count == 0)
                throw new ArgumentException("Generic parameters count must be greater than 0");

            GenericParameters = genericParameters;
            IsGenericType = true;
            IsGenericTypeDefinition = true;
        }

        public ClassView(
            string name
        ) : base(name)
        {
        }

        public void Configure(
            IReadOnlyCollection<TypePropertyView> properties
        )
        {
            if (_isConfigured)
                throw new InvalidOperationException("ClassView is already configured");

            Properties = properties;
            _isConfigured = true;
        }

        public ClassView MakeGenericType(
            params DefinedTypeView[] arguments
        )
        {
            if (!IsGenericTypeDefinition)
                throw new InvalidOperationException($"{this} is not generic type definition");

            if (arguments.Length != GenericParameters.Count)
                throw new InvalidOperationException($"Expected {GenericParameters.Count} generic arguments");

            var view = new ClassView(Name, arguments);
            var properties = Properties
                .Select(property =>
                {
                    if (!(property.Type is GenericParameterView))
                        return property;

                    var genericParameterPosition = GenericParameters
                        .Select((element, index) => (element, index))
                        .First(x => x.element == property.Type)
                        .index;

                    return new TypePropertyView(property.Name, arguments[genericParameterPosition], property.IsOptional);
                })
                .ToArray();
            view.Configure(properties);

            return view;
        }

        public override string ToString()
        {
            if (!IsGenericType)
                return Name;

            var arguments = IsGenericTypeDefinition ? (IReadOnlyCollection<TypeView>) GenericParameters : GenericArguments;

            return $"{Name}<{string.Join(", ", arguments)}>";
        }
    }
}