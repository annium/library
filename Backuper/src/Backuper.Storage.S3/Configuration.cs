using Annium.Extensions.Mapper;

namespace Backuper.Storage.S3
{
    [ResolveKey("s3")]
    public class Configuration : Abstract.Configuration
    {
        public string Path { get; set; }
    }
}