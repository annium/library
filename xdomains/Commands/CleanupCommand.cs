using System.Threading;
using Annium.Extensions.Arguments;
using xdomains.Tools;

namespace xdomains.Commands;

internal class CleanupCommand : Command<CleanupCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "cleanup";

    public static string Description => "cleanup whois cache";

    private readonly Cache _cache;

    public CleanupCommand(
        Cache cache
    )
    {
        _cache = cache;
    }

    public override void Handle(CleanupCommandConfiguration cfg, CancellationToken ct)
    {
        _cache.Cleanup();
    }
}

internal class CleanupCommandConfiguration
{
    [Option("f", isRequired : false)]
    public bool Force { get; set; }
}