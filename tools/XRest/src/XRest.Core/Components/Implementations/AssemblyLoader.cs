using System.Reflection;
using Annium.Core.Runtime.Loader;

namespace XRest.Core.Components.Implementations
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        public Assembly LoadFromPath(string assemblyPath)
        {
            var assembly = new PluginLoadContext(assemblyPath).LoadFromAssemblyPath(assemblyPath);

            return assembly;
        }
    }
}