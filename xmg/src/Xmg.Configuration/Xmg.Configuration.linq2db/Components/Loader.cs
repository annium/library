using System.IO;
using System.Reflection;
using Annium.linq2db.Extensions;
using LinqToDB.Mapping;
using LDatabase = Annium.linq2db.Extensions.Models.Database;

namespace Xmg.Configuration.linq2db.Components
{
    internal class Loader : ILoader
    {
        public LDatabase LoadMetadata(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            var assembly = Assembly.LoadFrom(assemblyPath);

            var mappingSchema = new MappingSchema();
            var mappingBuilder = mappingSchema.GetMappingBuilder(assembly);
            mappingBuilder.ApplyConfigurations().SnakeCaseColumns();

            return mappingBuilder.GetMetadata();
        }
    }
}