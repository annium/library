using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace xrest.Tools
{
    public class Generator
    {
        public void Generate(IReadOnlyCollection<Type> controllerTypes, string output)
        {
            foreach (var controllerType in controllerTypes)
            {
                var (methods, types) = ResolveControllerData(controllerType);
                Console.WriteLine("Generate methods");
                foreach (var method in methods)
                    Console.WriteLine($"Generate method {controllerType.Name}.{method.Name}");
                Console.WriteLine("Generate used types");
                foreach (var type in types)
                    Console.WriteLine($"Generate type {type.Name}");
            }
        }

        private (IReadOnlyCollection<MethodInfo>, IReadOnlyCollection<Type>) ResolveControllerData(Type controllerType)
        {
            var actionMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(Helper.IsAction)
                .ToArray();

            var types = actionMethods.SelectMany(ResolveTypes).Distinct().ToArray();

            return (actionMethods, types);
        }

        private IEnumerable<Type> ResolveTypes(MethodInfo action)
        {
            return action.GetParameters().SelectMany(x => ResolveTypes(x.ParameterType)).Concat(ResolveTypes(action.ReturnType));
        }

        private IEnumerable<Type> ResolveTypes(Type type)
        {
            // unmapped
            if (type == typeof(void) || type == typeof(object))
                return Enumerable.Empty<Type>();

            if (TypeMap.BaseTypes.ContainsKey(type))
                return Enumerable.Empty<Type>();

            // tasks
            if (type == typeof(Task))
                return Enumerable.Empty<Type>();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                return ResolveTypes(type.GenericTypeArguments[0]);

            // dictionary & array
            if (Helper.IsDictionary(type) || Helper.IsArray(type))
                return Enumerable.Empty<Type>();

            return type.GetProperties().Where(x => x.CanWrite && x.CanRead).SelectMany(x => ResolveTypes(x.PropertyType)).Append(type);
        }
    }
}