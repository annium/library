using Annium.Core.Application.Types;

namespace Backuper.Connection.Abstract
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; }
        public string Name { get; set; }
    }
}