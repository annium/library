using Annium.Extensions.Arguments;

namespace XLog.Commands.Graylog;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "graylog";
    public static string Description => "Graylog commands";

    public Group()
    {
        Add<DumpCommand>();
        Add<LoginCommand>();
    }
}
