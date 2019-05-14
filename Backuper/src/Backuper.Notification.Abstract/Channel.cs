using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Backuper.Shared;

namespace Backuper.Notification.Abstract
{
    public abstract class Channel : Resource
    {
        public Channel(
            string type,
            string name,
            ILogger logger
        ) : base(nameof(Channel), type, name, logger) { }

        public Task InfoAsync(string message) => SafeAsync("info", () => SendMessageAsync(LogLevel.Info, message));

        public Task WarnAsync(string message) => SafeAsync("warn", () => SendMessageAsync(LogLevel.Warn, message));

        public Task ErrorAsync(string message) => SafeAsync("error", () => SendMessageAsync(LogLevel.Error, message));

        protected abstract Task SendMessageAsync(LogLevel level, string message);
    }
}