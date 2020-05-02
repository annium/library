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

            var assembly = _loadContextFactory.Create(assemblyPath).LoadFromAssemblyPath(assemblyPath);

            var types = assembly.GetTypes();

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
    }
}