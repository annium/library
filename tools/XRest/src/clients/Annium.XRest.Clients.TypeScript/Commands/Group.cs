using Annium.Extensions.Arguments;

namespace Annium.XRest.Clients.TypeScript.Commands;

public class Group : Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "ts";

    public static string Description => "TypeScript commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}
