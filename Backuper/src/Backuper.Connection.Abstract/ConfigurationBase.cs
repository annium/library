using Annium.Core.Reflection;

namespace Backuper.Connection.Abstract
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}