using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Backuper.Shared;

namespace Backuper.Storage.Abstract
{
    public abstract class Storage : Resource
    {
        public Storage(
            string type,
            string name,
            ILogger logger
        ) : base(nameof(Storage), type, name, logger) { }

        public Task SetupAsync() => SafeAsync("setup", DoSetupAsync);

        protected abstract Task DoSetupAsync();

        public Task<string[]> ListAsync() => SafeAsync("list", DoListAsync);

        protected abstract Task<string[]> DoListAsync();

        public Task UploadAsync(string path, string name) => SafeAsync("upload", () => DoUploadAsync(path, name));

        protected abstract Task DoUploadAsync(string path, string name);

        public Task DownloadAsync(string name, string path) => SafeAsync("download", () => DoDownloadAsync(name, path));

        protected abstract Task DoDownloadAsync(string name, string path);

        public Task DeleteAsync(string name) => SafeAsync("upload", () => DoDeleteAsync(name));

        protected abstract Task DoDeleteAsync(string name);
    }
}