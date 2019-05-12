using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.S3
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }

        public override Task SetupAsync()
        {
            logger.Debug($"Setup S3 storage {Name}");

            return Task.CompletedTask;
        }

        public override Task<string[]> ListAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task UploadAsync(string path, string name)
        {
            throw new System.NotImplementedException();
        }

        public override Task DownloadAsync(string name, string path)
        {
            throw new System.NotImplementedException();
        }

        public override Task DeleteAsync(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}