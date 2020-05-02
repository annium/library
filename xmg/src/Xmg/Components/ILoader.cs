using System;
using System.Collections.Generic;

namespace Xmg.Components
{
    public interface ILoader
    {
        IReadOnlyCollection<(Type configurationType, Type entityType)> LoadConfigurationTypes(string assemblyPath);
    }
}