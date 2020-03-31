using System;
using XRest.Core.Models;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Components.Implementations
{
    internal class Processor : IProcessor
    {
        public ApiView Process(ApiModel api)
        {
            throw new NotImplementedException();
            // var raw = controllerTypes.Select(ParseController).ToArray();
            // var allTypes = raw.SelectMany(x => x.types).ToArray();

            // var data = new ApiModel();
            // // data.SharedExports = allTypes.Where((x, i) => Array.LastIndexOf(allTypes, x) != i).Distinct().ToArray();
            // // data.Services = raw.Select(x => BuildControllerData(x.name, x.methods, x.types, data.SharedExports)).ToArray();
            //
            // return data;
        }

        // private ControllerData BuildControllerData(
        //     string name,
        //     IReadOnlyCollection<MethodInfo> methods,
        //     IReadOnlyCollection<Type> types,
        //     IReadOnlyCollection<Type> sharedTypes
        // )
        // {
        //     var data = new ControllerData
        //     {
        //         Name = name,
        //         Methods = methods,
        //     };
        //     data.Exports = types.Except(sharedTypes).ToArray();
        //     data.Imports = types.Except(data.Exports).ToArray();
        //
        //     return data;
        // }
        //
        // private (string name, IReadOnlyCollection<MethodInfo> methods, IReadOnlyCollection<Type> types) ParseController(Type controllerType)
        // {
        //     var actionMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        //         .Where(Helper.IsAction)
        //         .ToArray();
        //
        //     var types = actionMethods.SelectMany(CollectTypes).Distinct().ToArray();
        //
        //     return (controllerType.Name.CamelCase(), actionMethods, types);
        // }
        //
        // private IEnumerable<Type> CollectTypes(MethodInfo action)
        // {
        //     return action.GetParameters().SelectMany(x => CollectTypes(x.ParameterType)).Concat(CollectTypes(action.ReturnType));
        // }
        //
        // private IEnumerable<Type> CollectTypes(Type type)
        // {
        //     // unmapped
        //     if (type == typeof(void) || type == typeof(object))
        //         return Enumerable.Empty<Type>();
        //
        //     if (TypeMap.BaseTypes.ContainsKey(type))
        //         return Enumerable.Empty<Type>();
        //
        //     // tasks
        //     if (type == typeof(Task))
        //         return Enumerable.Empty<Type>();
        //
        //     if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        //         return CollectTypes(type.GenericTypeArguments[0]);
        //
        //     // dictionary & array
        //     if (Helper.IsDictionary(type) || Helper.IsArray(type))
        //         return Enumerable.Empty<Type>();
        //
        //     return type.GetProperties().Where(x => x.CanWrite && x.CanRead).SelectMany(x => CollectTypes(x.PropertyType)).Append(type);
        // }
    }
}