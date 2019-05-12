using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.Abstract
{
    public abstract class Storage
    {
        public string Name { get; }

        protected readonly ILogger logger;

        public Storage(
            string name,
            ILogger logger
        )
        {
            Name = name;
            this.logger = logger;
        }

        public abstract Task SetupAsync();

        public abstract Task<string[]> ListAsync();

        public abstract Task UploadAsync(string path, string name);

        public abstract Task DownloadAsync(string name, string path);

        public abstract Task DeleteAsync(string name);
    }
}