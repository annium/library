using System;
using LinqToDB.Mapping;

namespace Xmg.Components
{
    public interface ILoader
    {
        (MappingSchema mappingSchema,Type[] entityTypes) LoadMappingSchema(string assemblyPath);
    }
}