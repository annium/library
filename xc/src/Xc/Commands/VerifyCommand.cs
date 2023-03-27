using System;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class VerifyCommand : Command<RootCommandConfiguration>
{
    public override string Id { get; } = "verify";
    public override string Description { get; } = "verify solution";

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}