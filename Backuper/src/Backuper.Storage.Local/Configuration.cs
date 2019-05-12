using Annium.Extensions.Mapper;

namespace Backuper.Storage.Local
{
    [ResolveKey("local")]
    public class Configuration : Abstract.Configuration
    {
        public string Path { get; set; }
    }
}