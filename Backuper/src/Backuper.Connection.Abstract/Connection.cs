using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.Abstract
{
    public abstract class Connection
    {
        public string Name { get; }

        protected readonly ILogger logger;

        public Connection(
            string name,
            ILogger logger
        )
        {
            Name = name;
            this.logger = logger;
        }

        public abstract Task SetupAsync();

        public abstract Task<string> BackupAsync();

        public abstract Task RestoreAsync(string path);
    }
}