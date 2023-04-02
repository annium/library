using Annium.Extensions.Arguments;

namespace XLog.Commands;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => string.Empty;
    public static string Description => "commands";

    public Group()
    {
        Add<ListenCommand>();
    }
}