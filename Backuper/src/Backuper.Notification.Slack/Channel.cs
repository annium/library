using Annium.Logging.Abstractions;

namespace Backuper.Notification.Slack
{
    public class Channel : Abstract.Channel
    {
        private readonly Configuration cfg;

        public Channel(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base(name, logger)
        {
            this.cfg = cfg;
        }
    }
}