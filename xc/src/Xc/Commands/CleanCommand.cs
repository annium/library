using System;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class CleanCommand : Command<RootCommandConfiguration>
{
    public override string Id { get; } = "clean";
    public override string Description { get; } = "clean solution";

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}