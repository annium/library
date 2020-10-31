using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands
{
    internal class ShowCommand : Command<RootCommandConfiguration>
    {
        public override string Id { get; } = "show";
        public override string Description { get; } = "show solution";

        public override void Handle(RootCommandConfiguration cfg, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}