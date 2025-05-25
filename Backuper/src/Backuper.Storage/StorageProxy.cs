using System.IO;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;
using Backuper.Shared;

namespace Backuper.Storage;

public class StorageProxy : Resource<IStorage>, IStorage
{
    public StorageProxy(IStorage entity, string type, ILogger logger)
        : base(entity, "Storage", type, logger) { }

    public Task<string[]> ListAsync(string prefix = "") => SafeAsync("list", () => Entity.ListAsync(prefix));

    public Task UploadAsync(Stream source, string name) => SafeAsync("upload", () => Entity.UploadAsync(source, name));

    public Task<Stream> DownloadAsync(string name) => SafeAsync("download", () => Entity.DownloadAsync(name));

    public Task<bool> DeleteAsync(string name) => SafeAsync("delete", () => Entity.DeleteAsync(name));
}
