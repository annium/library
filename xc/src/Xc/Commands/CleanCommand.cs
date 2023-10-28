using System;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class CleanCommand : Command<RootCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "clean";
    public static string Description => "clean solution";

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
