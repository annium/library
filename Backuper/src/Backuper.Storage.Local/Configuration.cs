using Annium.Core.Application.Types;

namespace Backuper.Storage.Local
{
    [ResolveKey("local")]
    public class Configuration : Abstract.Configuration
    {
        public string Path { get; set; }
    }
}