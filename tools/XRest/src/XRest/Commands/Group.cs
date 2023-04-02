using Annium.Extensions.Arguments;

namespace XRest.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xrest";

    public static string Description => "REST client generator";

    public Group()
    {
        Add<Clients.Csharp.Commands.Group>();
        Add<Clients.TypeScript.Commands.Group>();
        Add<ParseCommand>();
    }
}