using Annium.Extensions.Mapper;

namespace Backuper.Storage.Abstract
{
    public abstract class Configuration
    {
        [ResolveField]
        public string Type { get; set; }
    }
}