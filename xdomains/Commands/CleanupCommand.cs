using System.Threading;
using Annium.Extensions.Arguments;
using xdomains.Tools;

namespace xdomains.Commands
{
    internal class CleanupCommand : Command<CleanupCommandConfiguration>
    {
        public override string Id { get; } = "cleanup";

        public override string Description { get; } = "cleanup whois cache";

        private readonly Cache cache;

        public CleanupCommand(
            Cache cache
        )
        {
            this.cache = cache;
        }

        public override void Handle(CleanupCommandConfiguration cfg, CancellationToken ct)
        {
            cache.Cleanup();
        }
    }

    internal class CleanupCommandConfiguration
    {
        [Option("f", isRequired : false)]
        public bool Force { get; set; }
    }
}