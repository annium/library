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

        public Task<string[]> ListAsync(string folder) => SafeAsync("list", () => DoListAsync(folder));

        protected abstract Task<string[]> DoListAsync(string folder);

        public Task UploadAsync(string source, string folder, string name) => SafeAsync("upload", () => DoUploadAsync(source, folder, name));

        protected abstract Task DoUploadAsync(string source, string folder, string name);

        public Task DownloadAsync(string folder, string name, string target) => SafeAsync("download", () => DoDownloadAsync(folder, name, target));

        protected abstract Task DoDownloadAsync(string folder, string name, string target);

        public Task DeleteAsync(string folder, string name) => SafeAsync("upload", () => DoDeleteAsync(folder, name));

        protected abstract Task DoDeleteAsync(string folder, string name);
    }
}