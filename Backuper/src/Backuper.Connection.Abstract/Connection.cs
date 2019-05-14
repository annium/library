using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Backuper.Shared;

namespace Backuper.Connection.Abstract
{
    public abstract class Connection : Resource
    {
        public Connection(
            string type,
            string name,
            ILogger logger
        ) : base(nameof(Connection), type, name, logger) { }

        public Task SetupAsync() => SafeAsync("setup", DoSetupAsync);

        protected abstract Task DoSetupAsync();

        public Task<string> BackupAsync() => SafeAsync("backup", DoBackupAsync);

        protected abstract Task<string> DoBackupAsync();

        public Task RestoreAsync(string path) => SafeAsync("restore", () => DoRestoreAsync(path));

        protected abstract Task DoRestoreAsync(string path);
    }
}