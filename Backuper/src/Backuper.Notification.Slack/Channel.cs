using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
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
        ) : base("Slack", name, logger)
        {
            this.cfg = cfg;
        }

        protected override Task SendMessageAsync(LogLevel level, string message)
        {
            var url = $"https://hooks.slack.com/services/{cfg.Team}/{cfg.Channel}/{cfg.Token}";
            var text = $"{level} {message}";

            return Http.Open().Post(url).JsonContent(new { text }).RunAsync();
        }
    }
}