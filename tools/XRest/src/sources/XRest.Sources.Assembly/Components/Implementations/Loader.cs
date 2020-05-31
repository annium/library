using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using XRest.Core.Components;
using XRest.Core.Models;

namespace XRest.Sources.Assembly.Components.Implementations
{
    internal class Loader : ILoader
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IParser _parser;

        public Loader(
            IAssemblyLoader assemblyLoader,
            IParser parser
        )
        {
            _assemblyLoader = assemblyLoader;
            _parser = parser;
        }

        public ApiModel Load(string assemblyPath)
        {
            var controllerTypes = LoadControllerTypes(assemblyPath);

            return _parser.Parse(controllerTypes);
        }

        private IReadOnlyCollection<Type> LoadControllerTypes(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            return _assemblyLoader.LoadFromPath(assemblyPath).GetTypes()
                .Where(x => typeof(ControllerBase).IsAssignableFrom(x))
                .ToArray();
        }
    }
}