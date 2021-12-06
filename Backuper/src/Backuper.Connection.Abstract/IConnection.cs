using System.Threading.Tasks;

namespace Backuper.Connection.Abstract;

public interface IConnection
{
    Task SetupAsync();

    Task<string> BackupAsync();

    Task RestoreAsync(string path);
}