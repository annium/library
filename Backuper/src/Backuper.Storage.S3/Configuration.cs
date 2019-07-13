using Annium.Core.Application.Types;

namespace Backuper.Storage.S3
{
    [ResolveKey("s3")]
    public class Configuration : Abstract.Configuration
    {
        public string Server { get; set; }

        public string AccessKey { get; set; }

        public string AccessSecret { get; set; }

        public string Region { get; set; }

        public string Bucket { get; set; }

        public string EncryptionKey { get; set; }

        public string Path { get; set; }
    }
}