using System.Threading.Tasks;
using Annium.Logging;
using Backuper.Shared;

namespace Backuper.Connection.Abstract;

public class ConnectionProxy : Resource<IConnection>, IConnection
{
    public ConnectionProxy(
        IConnection entity,
        string type,
        ILogger logger
    ) : base(entity, "Connection", type, logger)
    {
    }

    public Task SetupAsync() => SafeAsync("info", Entity.SetupAsync);

    public Task<string> BackupAsync() => SafeAsync("info", Entity.BackupAsync);

    public Task RestoreAsync(string path) => SafeAsync("info", () => Entity.RestoreAsync(path));
}