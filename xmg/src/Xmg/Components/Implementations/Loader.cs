using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.Runtime.Loader;
using Annium.linq2db.Extensions;

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

            return LoadTypes(assemblyPath)
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
                .Select(x => (x, i: x.GetInterfaces().SingleOrDefault(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IEntityConfiguration<>))))
                .Where(p => p.i != null)
                .Select(p => (p.x, p.i.GenericTypeArguments.Single()))
                .ToArray();
        }

        private IReadOnlyCollection<Type> LoadTypes(string assemblyPath)
        {
            var assembly = _loadContextFactory.Create(assemblyPath).LoadFromAssemblyPath(assemblyPath);

            return assembly.GetTypes();
        }
    }
}