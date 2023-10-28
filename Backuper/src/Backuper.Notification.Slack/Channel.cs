using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http;
using Backuper.Notification.Abstract;

namespace Backuper.Notification.Slack;

public class Channel : IChannel
{
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly Configuration _cfg;

    public Channel(IHttpRequestFactory httpRequestFactory, Configuration cfg)
    {
        _httpRequestFactory = httpRequestFactory;
        _cfg = cfg;
    }

    public Task InfoAsync(string message) => SendMessageAsync(LogLevel.Info, message);

    public Task WarnAsync(string message) => SendMessageAsync(LogLevel.Warn, message);

    public Task ErrorAsync(string message) => SendMessageAsync(LogLevel.Error, message);

    private Task SendMessageAsync(LogLevel level, string message)
    {
        var url = $"https://hooks.slack.com/services/{_cfg.Team}/{_cfg.Channel}/{_cfg.Token}";
        var text = $"{level} {message}";

        return _httpRequestFactory.New().Post(url).JsonContent(new { text }).RunAsync();
    }
}
