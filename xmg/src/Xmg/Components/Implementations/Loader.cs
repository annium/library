using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Loader;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyModel;

namespace Xmg.Components.Implementations
{
    internal class Loader : ILoader
    {
        private const string libraryFileExtension = ".dll";
        private readonly DirectoryLoadContextFactory _loadContextFactory;

        public Loader(
            DirectoryLoadContextFactory loadContextFactory
        )
        {
            _loadContextFactory = loadContextFactory;
        }

        public (MappingSchema mappingSchema, Type[] entityTypes) LoadMappingSchema(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            var assembly = _loadContextFactory.Create(assemblyPath).LoadFromAssemblyPath(assemblyPath);
            // if (assembly.EntryPoint is null)
            //     throw new InvalidOperationException(
            //         $"Assembly '{assembly.GetName().Name}' has no Entrypoint. This is required to collect configurations from referenced assemblies");

            var types = LoadEntityTypes(assembly);
            var mappingSchema = new MappingSchema();
            // mappingSchema.GetMappingBuilder(assembly).ApplyConfigurations().SnakeCaseColumns();


            return (mappingSchema, types);
        }

        private Type[] CollectTypes(Assembly _assembly)
        {
            var core = typeof(object).Assembly.GetName();
            var assemblyNames = DependencyContext.Load(_assembly).CompileLibraries
                .Select(x => new AssemblyName(x.Name))
                .Prepend(core)
                .ToArray();

            var path = _assembly.Location;
            if (!File.Exists(path))
                return assemblyNames.SelectMany(GeneralAssemblyLoadTypes).ToArray();

            var directory = Path.GetDirectoryName(path)!;
            var loadContext = new DirectoryLoadContext(directory);
            Directory.SetCurrentDirectory(directory);

            return assemblyNames.SelectMany(LocatedAssemblyLoadTypes).ToArray();

            static Type[] GeneralAssemblyLoadTypes(AssemblyName name)
            {
                try
                {
                    return Assembly.Load(name).GetTypes();
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            }

            Type[] LocatedAssemblyLoadTypes(AssemblyName name)
            {
                var assemblyPath = Path.Combine(directory, $"{name.Name}{libraryFileExtension}");
                var fileExists = File.Exists(assemblyPath);
                var isSpecial = name.Name.Contains("Crypted");
                try
                {
                    if (fileExists)
                        return Assembly.LoadFrom(assemblyPath).GetTypes();

                    return Assembly.Load(name).GetTypes();
                }
                catch (FileNotFoundException)
                {
                    return Type.EmptyTypes;
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            }
        }


        private Type[] LoadEntityTypes(Assembly assembly)
        {
            var types = CollectTypes(assembly)
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
                .ToArray();

            var result = types
                .Select(x =>
                    x.GetInterfaces()
                        .SingleOrDefault(y =>
                            y.IsGenericType &&
                            y.GetGenericTypeDefinition().FullName!.Contains("IEntityConfiguration")
                        )?
                        .GenericTypeArguments
                        .Single()!
                )
                .Where(x => x != null)
                .ToArray();

            return result;
        }
    }
}