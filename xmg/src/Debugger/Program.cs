using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Annium.linq2db.Extensions;
using Debugger;

var path = "/projects/annium/id/server/src/Shared/Annium.Id.Db/bin/Debug/net9.0/Annium.Id.Db.dll";

var lc = new CustomLoadContext(path);
var assembly = lc.LoadFromAssemblyPath(path);
var types = assembly.GetTypes();

var cfgType = typeof(IEntityConfiguration<>);
var cfgTypeName = cfgType.AssemblyQualifiedName;

var exact = CollectTypes(assembly, x => x == cfgType);
var named = CollectTypes(assembly, x => x.AssemblyQualifiedName == cfgTypeName);
var namedCfgType = named.First().configurationType.GetInterfaces().First().GetGenericTypeDefinition();

Console.WriteLine("Done");

static (Type configurationType, Type entityType)[] CollectTypes(Assembly assembly, Predicate<Type> predicate)
{
    var allTypes = assembly.GetTypes();
    var concreteClasses = allTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType).ToArray();

    var configurationTypes = concreteClasses
        .Select(x =>
            (x, i: x.GetInterfaces().SingleOrDefault(i => i.IsGenericType && predicate(i.GetGenericTypeDefinition())))
        )
        .Where(p => p.i != null)
        .Select(p => (p.x, p.i!.GenericTypeArguments.Single()))
        .ToArray();

    return configurationTypes;
}

namespace Debugger
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string assemblyPath)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
        }
    }
}
