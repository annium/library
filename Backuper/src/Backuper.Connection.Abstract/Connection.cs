using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Connection.Abstract
{
    public abstract class Connection
    {
        public string Type { get; }

        public string Name { get; }

        private readonly ILogger logger;

        public Connection(
            string type,
            string name,
            ILogger logger
        )
        {
            Type = type;
            Name = name;
            this.logger = logger;
        }

        public async Task SetupAsync()
        {
            try
            {
                debug("setup start");
                await DoSetupAsync();
                debug("setup succeed");
            }
            catch
            {
                debug("setup failed");
                throw;
            }
        }

        protected abstract Task DoSetupAsync();

        public async Task<string> BackupAsync()
        {
            try
            {
                debug("backup start");
                var path = await DoBackupAsync();
                debug("backup succeed");

                return path;
            }
            catch
            {
                debug("backup failed");
                throw;
            }
        }

        protected abstract Task<string> DoBackupAsync();

        public async Task RestoreAsync(string path)
        {
            try
            {
                debug("restore start");
                await DoRestoreAsync(path);
                debug("restore succeed");
            }
            catch
            {
                debug("restore failed");
                throw;
            }
        }

        protected abstract Task DoRestoreAsync(string path);

        protected void debug(string message) => logger.Debug(msg(message));

        protected string msg(string message) => $"Connection {Type} {Name}: {message}";
    }
}