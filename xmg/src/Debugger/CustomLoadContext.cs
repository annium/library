using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Debugger;

public class CustomLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _builtInResolver;
    private readonly CustomAssemblyResolver _customResolver;
    private readonly CustomAssemblyResolver _customDefaultContextResolver;

    public CustomLoadContext(string lambdaPath)
        : base("CustomLoadContext")
    {
        _builtInResolver = new AssemblyDependencyResolver(lambdaPath);
        _customResolver = new CustomAssemblyResolver(this, lambdaPath);

        _customDefaultContextResolver = new CustomAssemblyResolver(Default, lambdaPath);

        Default.Resolving += OnDefaultAssemblyLoadContextResolving;
    }

    private Assembly? OnDefaultAssemblyLoadContextResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        var assemblyPath = _customDefaultContextResolver.ResolveAssemblyToPath(assemblyName);

        return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : default;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name!.StartsWith("Amazon.Lambda.Core"))
            return default;

        var assemblyPath = _builtInResolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath == null || !File.Exists(assemblyPath))
        {
            assemblyPath = _customResolver.ResolveAssemblyToPath(assemblyName);
        }

        return assemblyPath != null! ? LoadFromAssemblyPath(assemblyPath) : default;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _builtInResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    private class CustomAssemblyResolver
    {
        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext? _dependencyContext;

        public CustomAssemblyResolver(AssemblyLoadContext assemblyLoadContext, string rootAssemblyPath)
        {
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(rootAssemblyPath);
            _dependencyContext = DependencyContext.Load(assembly);

            _assemblyResolver = new CompositeCompilationAssemblyResolver(
                new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(rootAssemblyPath)!),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver()
                }
            );
        }

        public string? ResolveAssemblyToPath(AssemblyName name)
        {
            var library =
                _dependencyContext?.RuntimeLibraries.FirstOrDefault(NamesMatch)
                ?? _dependencyContext?.RuntimeLibraries.FirstOrDefault(ResourceAssetPathMatch);

            if (library is null)
                return default;

            var wrapper = new CompilationLibrary(
                library.Type,
                library.Name,
                library.Version,
                library.Hash,
                library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                library.Dependencies,
                library.Serviceable
            );

            var assemblies = new List<string>();
            _assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
            if (assemblies.Count > 0)
            {
                return assemblies[0];
            }

            return default!;

            bool ResourceAssetPathMatch(RuntimeLibrary runtime)
            {
                foreach (var group in runtime.RuntimeAssemblyGroups)
                {
                    foreach (var path in group.AssetPaths)
                    {
                        if (path.EndsWith("/" + name.Name + ".dll"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
