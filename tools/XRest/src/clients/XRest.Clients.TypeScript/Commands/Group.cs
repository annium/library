using Annium.Extensions.Arguments;

namespace XRest.Clients.TypeScript.Commands;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "ts";

    public static string Description => "TypeScript commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}
