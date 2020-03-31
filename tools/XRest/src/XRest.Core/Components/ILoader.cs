using System;
using System.Collections.Generic;

namespace XRest.Core.Components
{
    public interface ILoader
    {
        IReadOnlyCollection<Type> LoadControllerTypes(string assemblyPath);
    }
}