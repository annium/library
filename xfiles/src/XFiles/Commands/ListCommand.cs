using System.Threading;
using Annium.Extensions.Arguments;

namespace XFiles.Commands;

internal class ListCommand : Command, ICommandDescriptor
{
    public static string Id => "list";
    public static string Description => "list files";

    public override void Handle(CancellationToken ct)
    {
    }
}