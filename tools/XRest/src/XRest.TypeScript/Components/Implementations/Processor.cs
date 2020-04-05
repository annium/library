using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using XRest.Core.Models;
using XRest.TypeScript.Helpers;
using XRest.TypeScript.Views;

namespace XRest.TypeScript.Components.Implementations
{
    internal class Processor : IProcessor
    {
        public ApiView Process(ApiModel api)
        {
            var raw = api.Controllers.Select(ParseController).ToArray();
            var rawTypes = raw.SelectMany(x => x.types).Distinct().OrderBy(x => x.Name).ToArray();

            var typeRegistry = new TypeRegistry();
            var allTypes = typeRegistry.Register(rawTypes).Distinct().OrderBy(x => x.Name).ToArray();
            var controllers = raw.Select(x => (x.controller, types: x.types.Select(typeRegistry.Resolve).ToArray())).ToArray();

            var sharedExports = CollectSharedTypes(allTypes, controllers.Select(x => x.types).ToArray());
            var controllerViews = controllers.Select(x => BuildControllerView(typeRegistry, x.controller, sharedExports, x.types)).ToArray();

            return new ApiView(sharedExports, controllerViews);
        }

        private ControllerView BuildControllerView(
            TypeRegistry typeRegistry,
            ControllerModel model,
            IReadOnlyCollection<TypeView> sharedTypes,
            IReadOnlyCollection<TypeView> types
        )
        {
            var exports = types.Except(sharedTypes).ToArray();
            var imports = types.Except(exports).ToArray();

            var actions = model.Actions.Select(action => BuildActionView(typeRegistry, action)).ToArray();

            return new ControllerView(model.Name.CamelCase(), imports, actions, exports);
        }

        private ActionView BuildActionView(
            TypeRegistry typeRegistry,
            ActionModel model
        )
        {
            return new ActionView(
                model.Name,
                model.Method,
                model.Path,
                model.Parameters.Select(x => BuildParameterView(typeRegistry, x)).ToArray(),
                model.Body is null ? null : typeRegistry.Resolve(model.Body),
                model.Response is null ? null : typeRegistry.Resolve(model.Response),
                BuildAuthView(model.Auth)
            );
        }

        private ParameterView BuildParameterView(
            TypeRegistry typeRegistry,
            ParameterModel model
        )
        {
            return new ParameterView(
                model.Name,
                model.Location,
                typeRegistry.Resolve(model.Type)
            );
        }

        private AuthView BuildAuthView(
            AuthModel model
        )
        {
            return new AuthView(model.IsEnabled);
        }

        private IReadOnlyCollection<TypeView> CollectSharedTypes(
            IReadOnlyCollection<TypeView> allTypes,
            IReadOnlyCollection<TypeView[]> types
        )
        {
            var sharedTypes = new HashSet<TypeView>();

            foreach (var type in allTypes)
            {
                if (types.Count(x => x.Contains(type)) > 1)
                    CollectSharedTypes(type, sharedTypes.Add);
            }

            return sharedTypes;
        }

        private void CollectSharedTypes(
            TypeView type,
            Predicate<TypeView> register
        )
        {
            // skip built-in types
            if (Types.BuiltIn.Contains(type))
                return;

            // skip generic parameters
            if (type.IsGenericTypeParameter)
                return;

            // if not registered - already present in shared types
            if (!register(type))
                return;

            foreach (var property in type.Properties)
                CollectSharedTypes(property.Type, register);
        }

        private (ControllerModel controller, IReadOnlyCollection<Type> types) ParseController(ControllerModel controller)
        {
            var knownTypes = new HashSet<Type>();

            foreach (var action in controller.Actions)
                CollectTypes(action, knownTypes.Add);

            return (controller, knownTypes);
        }

        private void CollectTypes(ActionModel action, Predicate<Type> registerType)
        {
            foreach (var parameter in action.Parameters)
                CollectTypes(parameter.Type, registerType);

            CollectTypes(action.Body, registerType);
            CollectTypes(action.Response, registerType);
        }

        private void CollectTypes(Type? type, Predicate<Type> registerType)
        {
            if (type is null)
                return;

            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            var isCollected = !Types.BuiltIn.Contains(definition) && !definition.IsSkipped() && registerType(definition);

            if (type.IsGenericType)
                foreach (var typeArgument in type.GenericTypeArguments)
                    CollectTypes(typeArgument, registerType);

            if (isCollected)
                CollectPropertiesTypes(type, registerType);
        }

        private void CollectPropertiesTypes(Type type, Predicate<Type> registerType)
        {
            var propertyTypes = type.GetAllPublicProperties()
                .Select(x => x.PropertyType)
                .ToArray();

            foreach (var propertyType in propertyTypes)
                CollectTypes(propertyType, registerType);
        }
    }
}