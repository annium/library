using System.Reflection;
using Annium.Core.Runtime.Loader;

namespace XRest.Core.Components.Implementations
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly IPluginLoadContextFactory _loadContextFactory;

        public AssemblyLoader(
            IPluginLoadContextFactory loadContextFactory
        )
        {
            _loadContextFactory = loadContextFactory;
        }

        public Assembly LoadFromPath(string assemblyPath)
        {
            var assembly = _loadContextFactory.Create(assemblyPath).LoadFromAssemblyPath(assemblyPath);

            return assembly;
        }
    }
}