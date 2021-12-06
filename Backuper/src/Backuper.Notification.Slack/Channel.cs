using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Net.Http;
using Backuper.Notification.Abstract;

namespace Backuper.Notification.Slack;

public class Channel : IChannel
{
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly Configuration cfg;

    public Channel(
        IHttpRequestFactory httpRequestFactory,
        Configuration cfg
    )
    {
        _httpRequestFactory = httpRequestFactory;
        this.cfg = cfg;
    }

    public Task InfoAsync(string message) => SendMessageAsync(LogLevel.Info, message);

    public Task WarnAsync(string message) => SendMessageAsync(LogLevel.Warn, message);

    public Task ErrorAsync(string message) => SendMessageAsync(LogLevel.Error, message);

    private Task SendMessageAsync(LogLevel level, string message)
    {
        var url = $"https://hooks.slack.com/services/{cfg.Team}/{cfg.Channel}/{cfg.Token}";
        var text = $"{level} {message}";

        return _httpRequestFactory.New().Post(url).JsonContent(new { text }).RunAsync();
    }
}