using System.IO;
using System.Reflection;
using Annium.linq2db.Extensions;
using LinqToDB.Mapping;

namespace Xmg.Configuration.linq2db.Components
{
    internal class Loader : ILoader
    {
        public MappingSchema LoadMappingSchema(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file '{assemblyPath}' missing.");

            var assembly = Assembly.LoadFrom(assemblyPath);

            var mappingSchema = new MappingSchema();
            mappingSchema.GetMappingBuilder(assembly).ApplyConfigurations().SnakeCaseColumns();

            return mappingSchema;
        }
    }
}