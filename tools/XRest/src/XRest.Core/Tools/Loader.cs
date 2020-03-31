using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using XRest.Core.Helpers;

namespace XRest.Core.Tools
{
    public class Loader
    {
        public IReadOnlyCollection<Type> LoadControllerTypes(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            return LoadTypes(assemblyPath)
                .Where(x => typeof(ControllerBase).IsAssignableFrom(x))
                .ToArray();
        }

        private IReadOnlyCollection<Type> LoadTypes(string assemblyPath)
        {
            var assembly = new PluginLoadContext(assemblyPath).LoadFromAssemblyPath(assemblyPath);

            return assembly.GetTypes();
        }
    }
}