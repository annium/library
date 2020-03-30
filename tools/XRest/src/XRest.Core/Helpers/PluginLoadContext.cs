using System.Reflection;
using System.Runtime.Loader;

namespace XRest.Core.Helpers
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
        }
    }
}