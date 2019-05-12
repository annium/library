using Annium.Logging.Abstractions;

namespace Backuper.Notification.Abstract
{
    public abstract class Channel
    {
        public string Name { get; }

        protected readonly ILogger logger;

        public Channel(
            string name,
            ILogger logger
        )
        {
            Name = name;
            this.logger = logger;
        }
    }
}