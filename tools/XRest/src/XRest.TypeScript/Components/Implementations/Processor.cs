using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using XRest.Core.Models;
using XRest.TypeScript.Helpers;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Components.Implementations
{
    internal class Processor : IProcessor
    {
        public ApiView Process(ApiModel api)
        {
            var raw = api.Controllers.Select(ParseController).ToArray();

            var typeRegistry = new TypeRegistry();
            var allTypes = typeRegistry.Register(raw.SelectMany(x => x.types).Distinct().ToArray());
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
            // if not registered - already present in shared types
            if (!register(type))
                return;

            // if (!(type.BaseType is null))
            //     register(type.BaseType);
            //
            // foreach (var @interface in type.Interfaces)
            //     register(@interface);

            foreach (var property in type.Properties)
                if (!TypeMap.BaseTypes.ContainsValue(property.Type) && !type.GenericParameters.Contains(property.Type))
                    register(property.Type);
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

            if (CollectDefinitionTypes(type, registerType))
            {
                // CollectInheritanceTypes(type, registerType);
                CollectPropertyTypes(type, registerType);
            }
        }

        private bool CollectDefinitionTypes(Type type, Predicate<Type> registerType)
        {
            // if not generic type - return true, if type is not skipped and registered
            if (!type.IsGenericType)
                return !IsTypeSkipped(type) && registerType(type);

            foreach (var typeArgument in type.GenericTypeArguments)
                CollectTypes(typeArgument, registerType);

            var definition = type.GetGenericTypeDefinition();

            return !IsTypeSkipped(definition) && registerType(definition);
        }

        private void CollectPropertyTypes(Type type, Predicate<Type> registerType)
        {
            var propertyTypes = ProcessorHelper.GetProperties(type)
                .Select(x => x.PropertyType)
                .ToArray();

            foreach (var propertyType in propertyTypes)
                CollectTypes(propertyType, registerType);
        }

        private bool IsTypeSkipped(Type type)
        {
            return type == typeof(void) ||
                type == typeof(object) ||
                TypeMap.BaseTypes.Keys.Any(x => x.FullName == type.FullName) ||
                TypeMap.SkippedTypes.Any(x => x.FullName == type.FullName) ||
                ProcessorHelper.IsDictionary(type) ||
                ProcessorHelper.IsArray(type);
        }
    }
}