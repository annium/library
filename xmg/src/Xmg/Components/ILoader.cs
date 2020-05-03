using LinqToDB.Mapping;

namespace Xmg.Components
{
    public interface ILoader
    {
        MappingSchema LoadMappingSchema(string assemblyPath);
    }
}