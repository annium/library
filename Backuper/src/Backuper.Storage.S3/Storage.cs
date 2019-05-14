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
        ) : base("S3", name, logger)
        {
            this.cfg = cfg;
        }

        protected override Task DoSetupAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task<string[]> DoListAsync()
        {
            debug("Setup");

            throw new System.NotImplementedException();
        }

        protected override Task DoUploadAsync(string path, string name)
        {
            throw new System.NotImplementedException();
        }

        protected override Task DoDownloadAsync(string name, string path)
        {
            throw new System.NotImplementedException();
        }

        protected override Task DoDeleteAsync(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}