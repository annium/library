using Annium.Extensions.Mapper;

namespace Backuper.Connection.Abstract
{
    public abstract class Configuration
    {
        [ResolveField]
        public string Type { get; set; }
    }
}