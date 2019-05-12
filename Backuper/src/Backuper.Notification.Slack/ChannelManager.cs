using System.Threading.Tasks;

namespace Backuper.Notification.Slack
{
    public class ChannelManager : Abstract.ChannelManager<Configuration>
    {
        public override Task<Abstract.Channel> GetChannelAsync(Configuration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}