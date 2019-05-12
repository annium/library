using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Backuper.Notification.Slack
{
    public class ChannelManager : Abstract.ChannelManager<Configuration>
    {
        private readonly ILogger<Channel> logger;

        public ChannelManager(
            ILogger<Channel> logger
        )
        {
            this.logger = logger;
        }

        public override Task<Abstract.Channel> GetChannelAsync(string name, Configuration configuration)
        {
            return Task.FromResult<Abstract.Channel>(new Channel(name, configuration, logger));
        }
    }
}