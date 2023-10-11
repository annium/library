using Annium.Extensions.Arguments;

namespace XFiles.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xc";

    public static string Description => "Configuration manager";

    public Group()
    {
        Add<ListCommand>();
    }
}