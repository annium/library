using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Annium.Core.Runtime.Loader;
using Annium.linq2db.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace Xmg.Components.Implementations
{
    internal class Loader : ILoader
    {
        private readonly DirectoryLoadContextFactory _loadContextFactory;

        public Loader(
            DirectoryLoadContextFactory loadContextFactory
        )
        {
            _loadContextFactory = loadContextFactory;
        }

        public IReadOnlyCollection<(Type configurationType, Type entityType)> LoadConfigurationTypes(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            var loadContext = _loadContextFactory.Create(assemblyPath);
            var types = CollectTypes(loadContext, assemblyPath).SelectMany(x => x).Distinct().ToArray();

            return types
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
                .Select(x => (
                    x,
                    i: x.GetInterfaces()
                        .SingleOrDefault(y =>
                            y.IsGenericType &&
                            y.GetGenericTypeDefinition().FullName == typeof(IEntityConfiguration<>).FullName
                        )
                ))
                .Where(p => p.i != null)
                .Select(p => (p.x, p.i.GenericTypeArguments.Single()))
                .ToArray();
        }

        private IEnumerable<Type[]> CollectTypes(AssemblyLoadContext loadContext, string assemblyPath)
        {
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var compileLibraryNames = DependencyContext.Load(assembly).CompileLibraries.Select(x => x.Name).ToArray();
            var directory = Path.GetDirectoryName(assemblyPath);

            // fetch types from discoverable compile libraries (assembly must be among them, so processed as others)
            foreach (var libraryName in compileLibraryNames)
            {
                if (!File.Exists(Path.Combine(directory, $"{libraryName}.dll")))
                    continue;

                var libary = loadContext.LoadFromAssemblyName(new AssemblyName(libraryName));
                yield return libary.GetTypes();
            }
        }

        //
        // private IEnumerable<Type> CollectTypes(AssemblyLoadContext loadContext, Assembly assembly)
        // {
        //     var types = assembly.GetTypes();
        //
        //     var references = assembly.GetReferencedAssemblies().Select(loadContext.LoadFromAssemblyName).ToArray();
        //
        //     return references.SelectMany(x => CollectTypes(loadContext, x)).Concat(types).ToArray();
        // }
    }
}