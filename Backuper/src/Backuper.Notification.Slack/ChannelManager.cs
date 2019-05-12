using System.Threading.Tasks;

namespace Backuper.Notification.Slack
{
    public class ChannelManager : Abstract.ChannelManager<Configuration>
    {
        public override Task<Abstract.Channel> GetChannelAsync(string name, Configuration configuration)
        {
            return Task.FromResult<Abstract.Channel>(new Channel(name));
        }
    }
}