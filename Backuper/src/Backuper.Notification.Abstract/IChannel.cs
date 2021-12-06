using System.Threading.Tasks;

namespace Backuper.Notification.Abstract;

public interface IChannel
{
    Task InfoAsync(string message);

    Task WarnAsync(string message);

    Task ErrorAsync(string message);
}