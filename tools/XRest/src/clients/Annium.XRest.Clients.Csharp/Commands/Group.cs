using Annium.Extensions.Arguments;

namespace Annium.XRest.Clients.Csharp.Commands;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "cs";

    public static string Description => "C# commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}
