using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands
{
    internal class ConfigureCommand : Command<RootCommandConfiguration>
    {
        public override string Id { get; } = "configure";
        public override string Description { get; } = "configure solution";

        public override void Handle(RootCommandConfiguration cfg, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}