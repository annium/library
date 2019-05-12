using Annium.Logging.Abstractions;

namespace Backuper.Notification.Slack
{
    public class Channel : Abstract.Channel
    {
        public Channel(
            string name,
            ILogger logger
        ) : base(name, logger)
        {

        }
    }
}