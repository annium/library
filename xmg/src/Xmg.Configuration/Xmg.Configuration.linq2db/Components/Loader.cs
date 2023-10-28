using System;
using System.IO;
using Annium.Core.Runtime.Loader;
using Annium.linq2db.Extensions;

namespace Xmg.Configuration.linq2db.Components;

internal class Loader : ILoader
{
    private readonly IAssemblyLoaderBuilder _assemblyLoaderBuilder;

    public Loader(IAssemblyLoaderBuilder assemblyLoaderBuilder)
    {
        _assemblyLoaderBuilder = assemblyLoaderBuilder;
    }

    public DatabaseMetadata LoadMetadata(string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

        _assemblyLoaderBuilder.UseFileSystemLoader(Path.GetDirectoryName(assemblyPath)!).Build();
        throw new NotImplementedException();
        // _assembly = _loadContextFactory.Create(assemblyPath).LoadFromAssemblyPath(assemblyPath);
        //
        // var mappingSchema = Resolve<MappingSchema>();
        //
        // var mappingSchemaExtensions = ResolveType(typeof(MappingSchemaExtensions));
        // var builder = mappingSchemaExtensions.GetMethod(nameof(MappingSchemaExtensions.GetMappingBuilder))!
        //     .Invoke(null, new[] { mappingSchema, _assembly });
        //
        // var builderType = ResolveType(typeof(MappingBuilder));
        // var builderExtensionsType = ResolveType(typeof(MappingBuilderExtensions));
        //
        // builderType.GetMethod(nameof(MappingBuilder.ApplyConfigurations))!
        //     .Invoke(builder, Array.Empty<object>());
        // builderExtensionsType.GetMethod(nameof(MappingBuilderExtensions.SnakeCaseColumns))!
        //     .Invoke(null, new[] { builder });
        //
        // var metadata = builderType.GetMethod(nameof(MappingBuilder.GetMetadata))!
        //     .Invoke(builder, new[] { Resolve<MetadataBuilderFlags>() });
        //
        //
        // throw new NotImplementedException();
        // var mappingBuilder = mappingSchema.GetMappingBuilder(assembly);
        // mappingBuilder.ApplyConfigurations().SnakeCaseColumns();
        //
        // return mappingBuilder.GetMetadata();
    }
    //
    // private object Resolve<T>()
    // {
    //     var types = TypeManager.GetInstance(_assembly).Types;
    //     var type = types.Single(x => x.AssemblyQualifiedName == typeof(T).AssemblyQualifiedName);
    //
    //     return Activator.CreateInstance(type)!;
    // }
    //
    // private Type ResolveType(Type type)
    // {
    //     var types = TypeManager.GetInstance(_assembly).Types;
    //
    //     return types.Single(x => x.AssemblyQualifiedName == type.AssemblyQualifiedName);
    // }
}
