using System;
using System.Collections.Generic;
using System.Linq;

namespace XRest.Clients.TypeScript.Views.Types;

internal record ClassView : DefinedTypeView
{
    public override TypeViewEnum Type => TypeViewEnum.Class;
    public bool IsGenericType { get; }
    public bool IsGenericTypeDefinition { get; private set; }
    public IReadOnlyCollection<DefinedTypeView> GenericArguments { get; } = Array.Empty<DefinedTypeView>();
    public IReadOnlyCollection<GenericParameterView> GenericParameters { get; } = Array.Empty<GenericParameterView>();
    public IReadOnlyCollection<TypePropertyView> Properties { get; private set; } = Array.Empty<TypePropertyView>();
    private IReadOnlyCollection<TypePropertyView> _definitionProperties = Array.Empty<TypePropertyView>();
    private bool _isConfigured;

    public ClassView(string name, IReadOnlyCollection<GenericParameterView> genericParameters)
        : this(name)
    {
        if (genericParameters.Count == 0)
            throw new ArgumentException("Generic parameters count must be greater than 0");

        GenericParameters = genericParameters;
        IsGenericType = true;
        IsGenericTypeDefinition = true;
    }

    public ClassView(string name)
        : base(name) { }

    private ClassView(
        string name,
        IReadOnlyCollection<GenericParameterView> genericParameters,
        IReadOnlyCollection<DefinedTypeView> genericArguments
    )
        : this(name, genericParameters)
    {
        GenericArguments = genericArguments;
        IsGenericTypeDefinition = false;
    }

    public ClassView Configure(IReadOnlyCollection<TypePropertyView> properties)
    {
        if (_isConfigured)
            throw new InvalidOperationException("ClassView is already configured");

        Properties = properties;
        _isConfigured = true;

        return this;
    }

    public ClassView Configure(
        IReadOnlyCollection<TypePropertyView> definitionProperties,
        IReadOnlyCollection<TypePropertyView> properties
    )
    {
        if (_isConfigured)
            throw new InvalidOperationException("ClassView is already configured");

        _definitionProperties = properties;
        Properties = properties;
        _isConfigured = true;

        return this;
    }

    public ClassView MakeGenericType(params DefinedTypeView[] arguments)
    {
        if (!IsGenericTypeDefinition)
            throw new InvalidOperationException($"{this} is not generic type definition");

        if (arguments.Length != GenericParameters.Count)
            throw new InvalidOperationException($"Expected {GenericParameters.Count} generic arguments");

        var view = new ClassView(Name, GenericParameters, arguments);
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
        view.Configure(_definitionProperties, properties);

        return view;
    }

    public ClassView GetGenericDefinition()
    {
        if (!IsGenericType)
            throw new InvalidOperationException($"{this} is not generic type");

        if (IsGenericTypeDefinition)
            return this;

        return new ClassView(Name, GenericParameters).Configure(_definitionProperties, Array.Empty<TypePropertyView>());
    }

    public IReadOnlyCollection<DefinedTypeView> GetPropertyTypes() =>
        Properties.Select(x => x.Type).OfType<DefinedTypeView>().Distinct().ToArray();

    public override string ToString()
    {
        if (!IsGenericType)
            return Name;

        var arguments = IsGenericTypeDefinition ? (IReadOnlyCollection<TypeView>)GenericParameters : GenericArguments;

        // special handling for nullable
        if (Name == BaseType.Nullable.Name)
            return arguments.Single().ToString()!;

        // special handling for arrays
        if (Name == BaseType.Array.Name)
            return $"{arguments.Single()}[]";

        return arguments.Count > 0 ? $"{Name}<{string.Join(", ", arguments)}>" : Name;
    }
}
