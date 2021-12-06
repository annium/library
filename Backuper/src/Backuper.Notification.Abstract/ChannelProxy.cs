using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Backuper.Shared;

namespace Backuper.Notification.Abstract;

public class ChannelProxy : Resource<IChannel>, IChannel
{
    public ChannelProxy(
        IChannel entity,
        string type,
        ILogger logger
    ) : base(entity, "Channel", type, logger) { }

    public Task InfoAsync(string message) => SafeAsync("info", () => Entity.InfoAsync(message));

    public Task WarnAsync(string message) => SafeAsync("warn", () => Entity.WarnAsync(message));

    public Task ErrorAsync(string message) => SafeAsync("error", () => Entity.ErrorAsync(message));
}