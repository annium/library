using System.Threading.Tasks;

namespace Backuper.Notification.Abstract
{
    public abstract class ChannelManager<T> where T : Configuration
    {
        public abstract Task<Channel> GetChannelAsync(T configuration);
    }
}