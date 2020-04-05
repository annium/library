using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using XRest.Core.Models;
using XRest.TypeScript.Helpers;
using XRest.TypeScript.Views;
using XRest.TypeScript.Views.Types;

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

            var sharedExports = CollectSharedTypes(allTypes, controllers.Select(x => x.types).ToArray()).OrderBy(x => x.Name).ToArray();
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
            IReadOnlyCollection<DefinedTypeView> allTypes,
            IReadOnlyCollection<DefinedTypeView[]> types
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
            DefinedTypeView type,
            Predicate<DefinedTypeView> register
        )
        {
            // skip built-in types
            if (KnownTypes.BuiltIn.Contains(type))
                return;

            // if not registered - already present in shared types
            if (!register(type))
                return;

            if (type is ClassView classType)
                foreach (var propertyType in classType.Properties.Select(x => x.Type).OfType<DefinedTypeView>())
                    CollectSharedTypes(propertyType, register);
        }

        private (ControllerModel controller, IReadOnlyCollection<Type> types) ParseController(ControllerModel controller)
        {
            var collector = new TypeCollector();

            foreach (var action in controller.Actions)
            {
                foreach (var parameter in action.Parameters)
                    collector.CollectTypes(parameter.Type);

                collector.CollectTypes(action.Body);
                collector.CollectTypes(action.Response);
            }

            return (controller, collector.CollectedTypes);
        }
    }
}