using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.Runtime.Loader;
using Microsoft.AspNetCore.Mvc;
using XRest.Core.Models;

namespace XRest.Sources.Assembly.Components.Implementations
{
    internal class Loader : ILoader
    {
        private readonly IAssemblyLoaderBuilder _assemblyLoaderBuilder;
        private readonly IParser _parser;

        public Loader(
            IAssemblyLoaderBuilder assemblyLoaderBuilder,
            IParser parser
        )
        {
            _assemblyLoaderBuilder = assemblyLoaderBuilder;
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

            var loader = _assemblyLoaderBuilder.UseFileSystemLoader(Path.GetDirectoryName(assemblyPath)!).Build();
            var name = Path.GetFileNameWithoutExtension(assemblyPath);

            return loader.Load(name)
                .GetTypes()
                .Where(x => typeof(ControllerBase).IsAssignableFrom(x))
                .ToArray();
        }
    }
}