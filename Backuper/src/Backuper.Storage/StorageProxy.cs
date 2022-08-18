using System.IO;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Backuper.Shared;

namespace Backuper.Storage;

public class StorageProxy : Resource<IStorage>, IStorage
{
    public StorageProxy(
        IStorage entity,
        string type,
        ILogger<StorageProxy> logger
    ) : base(entity, "Storage", type, logger)
    {
    }

    public Task SetupAsync() => SafeAsync("setup", Entity.SetupAsync);

    public Task<string[]> ListAsync() => SafeAsync("list", Entity.ListAsync);

    public Task UploadAsync(Stream source, string name) => SafeAsync("upload", () => Entity.UploadAsync(source, name));

    public Task<Stream> DownloadAsync(string name) => SafeAsync("download", () => Entity.DownloadAsync(name));

    public Task<bool> DeleteAsync(string name) => SafeAsync("delete", () => Entity.DeleteAsync(name));
}