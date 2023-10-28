using System;
using System.Threading;
using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class ConfigureCommand : Command<RootCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "configure";
    public static string Description => "configure solution";

    public override void Handle(RootCommandConfiguration cfg, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
