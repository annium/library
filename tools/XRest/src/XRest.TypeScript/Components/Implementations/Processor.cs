using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var allTypes = raw.SelectMany(x => x.types).ToArray();

            var sharedExports = allTypes.Where((x, i) => Array.LastIndexOf(allTypes, x) != i).Distinct().ToArray();
            var controllerViews = raw.Select(x => BuildControllerView(x.controller, x.types, sharedExports)).ToArray();

            return new ApiView(sharedExports, controllerViews);
        }

        private ControllerView BuildControllerView(
            ControllerModel controller,
            IReadOnlyCollection<Type> types,
            IReadOnlyCollection<Type> sharedTypes
        )
        {
            var exports = types.Except(sharedTypes).ToArray();
            var imports = types.Except(exports).ToArray();

            return new ControllerView(controller.Name.CamelCase(), imports, controller.Actions, exports);
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
            var propertyTypes = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(x => x.CanRead)
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