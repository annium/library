using LinqToDB.Mapping;

namespace Xmg.Configuration.linq2db.Components
{
    public interface ILoader
    {
        MappingSchema LoadMappingSchema(string assemblyPath);
    }
}