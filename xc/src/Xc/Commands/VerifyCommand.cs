using System;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class VerifyCommand : Command<RootCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "verify";
    public static string Description => "verify solution";

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
