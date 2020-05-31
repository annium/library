using System.Reflection;

namespace XRest.Core.Components
{
    public interface IAssemblyLoader
    {
        Assembly LoadFromPath(string assemblyPath);
    }
}